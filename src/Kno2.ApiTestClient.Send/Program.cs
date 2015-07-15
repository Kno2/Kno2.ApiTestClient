using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Kno2.ApiTestClient.Core;
using Kno2.ApiTestClient.Core.Helpers;
using Kno2.ApiTestClient.Core.Resources;
using Kno2.ApiTestClient.Send.Extensions;

namespace Kno2.ApiTestClient.Send
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Initialize the configuration data
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Initializing Configuration".AsOpeningBanner(ConsoleColor.DarkGray, false);
                ApiConfig apiConfig = new ApiConfig();
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Create a reusable HttpClient
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Creating Authenticated Http Client".AsInlineBanner(ConsoleColor.Gray);
                HttpClient httpClient = HttpClientHelper.CreateHttpClient(baseUri: apiConfig.BaseUri,
                    defaultAccept: "application/json",
                    clientId: apiConfig.ClientId,
                    clientSecret: apiConfig.ClientSecret,
                    appId: apiConfig.AppId,
                    authUri: apiConfig.AuthUri,
                    grantType: "client_credentials",
                    emrSessionValue: apiConfig.EmrSessionValue
                    );
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --


                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Create a stock patient info
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Creating stock patient info".AsInlineBanner(ConsoleColor.Gray);
                var stockPatient = new Patient
                {
                    PatientId = "8675309",
                    FirstName = "John",
                    LastName = "Smith (emr-client)",
                    Gender = "M",
                    BirthDate = new DateTime(1980, 1, 1).ToShortDateString()
                };
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Validate the associated addresses
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                string toAddress = "results@" + apiConfig.DirectMessageDomain;
                string fromAddress = "referral@" + apiConfig.DirectMessageDomain;
                "Validating Addresses".AsInlineBanner(dark);
                Dictionary<string, bool> addressValidationResults =
                    ApiHelper.ValidateAddresses(httpClient: httpClient,
                        directoryValidateUri: apiConfig.DirectoryValidate(),
                        addresses: new[] { toAddress, fromAddress });
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Request the available document types
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Requesting Available Document Types".AsInlineBanner(light);
                IEnumerable<string> documentTypes = ApiHelper.RequestDocumentTypes(httpClient: httpClient,
                    documentTypesUri: apiConfig.DocumentTypesUri());
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Request a message draft id
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Requesting Draft Id".AsInlineBanner(dark);
                var outboundMessage = ApiHelper.RequestMessageDraft(httpClient: httpClient,
                      messageUri: apiConfig.MessagesUri()
                    );
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Upload an attachment using the draft id
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                var attachmentIds = new List<string>();
                foreach (var fileType in Enum.GetValues(typeof(FileType)).Cast<FileType>())
                {
                    string fileName = FileHelpers.GenerateAttachmentName(fileType);
                    ("Uploading " + fileType + " Attachment " + fileName).AsInlineBanner(light);
                    var attachment = ApiHelper.UploadAttachment(httpClient: httpClient,
                        attachmentsUri: apiConfig.AttachmentsUri(outboundMessage.Id),
                        fileName: fileName,
                        attachment: new AttachmentResource
                        {
                            NativeFileName = fileName,
                            NativeFileBytes = FileHelpers.GetSampleAttachmentBytes(fileType),
                            DocumentType = documentTypes.First(),
                            AttachmentMeta = new AttachmentMetaResource
                            {
                                DocumentTitle = fileType.Description() + " Sample Document Title",
                                DocumentType = documentTypes.First(),
                                DocumentDate = DateTime.UtcNow,
                                DocumentDescription = fileType.Description() + " Sample Document Description",
                                Confidentiality = Confidentiality.Normal
                            }
                        }
                        );
                    // The UploadAttachment helper method and the underlying API will give us the full attachment object
                    //  back but for the sake of the metadata requests we're only storing the Ids at this point
                    attachmentIds.Add(attachment.Id);
                }
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Request the attachment meta data
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                var attachments = new List<AttachmentResource>();
                foreach (var id in attachmentIds)
                {
                    ("Requesting Attachment Metadata for attachment " + id).AsInlineBanner(dark);
                    var metadata = ApiHelper.RequestAttachmentMetadata(httpClient: httpClient,
                        attachmentsUri: apiConfig.AttachmentsUri(messageId: outboundMessage.Id, attachmentId: id)
                        );
                    attachments.Add(metadata);
                }
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Send the message (draft)
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Sending the message (draft)".AsInlineBanner(light);

                outboundMessage.Attachments = attachments;
                outboundMessage.Subject = "Referral";
                outboundMessage.ToAddress = toAddress;
                outboundMessage.FromAddress = fromAddress;
                outboundMessage.Patient = stockPatient;

                ApiHelper.SendDraft(httpClient: httpClient,
                    messageUri: apiConfig.MessagesUri(outboundMessage.Id),
                    messageResource: outboundMessage);
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Updating the message (draft)
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Updating the message (draft)".AsInlineBanner(light);

                outboundMessage.Patient.MiddleName = " (emr-client)";
                outboundMessage.Patient.LastName = "Smith";

                ApiHelper.SendDraft(httpClient: httpClient,
                    messageUri: apiConfig.MessagesUri(outboundMessage.Id),
                    messageResource: outboundMessage);
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                // Send the message (release)
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                "Sending the message (release)".AsInlineBanner(dark);

                outboundMessage.Comments = "Comments";
                outboundMessage.ReasonForDisclosure = "Additional disclosure reason ";
                outboundMessage.Body = "Referral from Caring Hands Village\n\n" +
                                    "Referral for: \nPatient ID: 8675309\n" +
                                    "Patient Name: John Smith\nDOB: 01/01/1980\n\n" +
                                    "Comments:\nComments";

                ApiHelper.SendRelease(httpClient: httpClient,
                    messageSendUri: apiConfig.MessageSendUri(outboundMessage.Id),
                    messageResource: outboundMessage
                    );
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --


            }
            catch (AggregateException ex)
            {
                "Error".AsOpeningBanner(ConsoleColor.Red);
                foreach (var innerException in ex.InnerExceptions)
                {
                    innerException.Message.ToConsole(ConsoleColor.Red);
                    if (innerException.InnerException != null)
                    {
                        (" - " + innerException.InnerException.Message).ToConsole(ConsoleColor.Red);
                    }
                }
                string.Empty.AsClosingBanner(ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                ex.Message.AsBanner(ConsoleColor.Red);
            }

            Console.ResetColor();
        }

        static ConsoleColor light = ConsoleColor.Cyan;
        static ConsoleColor dark = ConsoleColor.DarkCyan;

    }
}
