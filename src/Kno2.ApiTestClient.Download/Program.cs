using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters;
using Kno2.ApiTestClient.Core;
using Kno2.ApiTestClient.Core.Helpers;
using Kno2.ApiTestClient.Core.Resources;
using Newtonsoft.Json;

namespace Kno2.ApiTestClient.Download
{
    class Program
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
                // Request the unprocessed intake messages
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --

                // This will wait forever for messages that meet the search criteria
                IEnumerable<MessageResource> intakeMessages = Enumerable.Empty<MessageResource>();
                while (true)
                {
                    "Requesting Available Unprocessed Intake Messages".AsOpeningBanner(light, false, true, false, false);
                    intakeMessages = ApiHelper.RequestUnprocessedIntakeMessages(httpClient: httpClient,
                        documentsMessagesUri: apiConfig.MessageSearch());



                    // Message Download / Recieve


                    // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                    // Set the message output to be next the executable
                    // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                    string messageOutputPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                    // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --


                    foreach (var intakeMessage in intakeMessages)
                    {
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        // Request the message
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        "Getting Message".AsInlineBanner(dark);
                        string messageJson = ApiHelper.RequestMessage(httpClient: httpClient,
                                                        messageUri: apiConfig.MessagesUri(intakeMessage.Id)
                                                        );
                        string localMessageDirectory = Path.Combine(messageOutputPath, "MessageDownload", intakeMessage.Id);
                        if (!Directory.Exists(localMessageDirectory))
                            Directory.CreateDirectory(localMessageDirectory);

                        File.WriteAllText(Path.Combine(localMessageDirectory, "message.json"), messageJson);
                        var retrievedMessage = JsonConvert.DeserializeObject<MessageResource>(messageJson);
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        // Request the attachment meta data
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        foreach (var attachment in retrievedMessage.Attachments)
                        {
                            ("Requesting Attachment Metadata for attachment " + attachment.Id).AsInlineBanner(dark);
                            var metadata = ApiHelper.RequestAttachmentMetadata(httpClient: httpClient,
                                attachmentsUri: apiConfig.AttachmentsUri(messageId: intakeMessage.Id, attachmentId: attachment.Id)
                                );
                            string filedata = ApiHelper.Serialize(metadata, httpClient.DefaultMediaType());
                            string fileName = Path.Combine(messageOutputPath, localMessageDirectory, attachment.NativeFileName + ".metadata." + httpClient.DefaultMediaType());
                            (" √ saving metadata file as " + fileName).AsClosingBanner(light);
                            File.WriteAllText(fileName, filedata);
                        }
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        // Request the native attachment then save it to disk.
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        foreach (var attachment in intakeMessage.Attachments.OrderBy(a => a.IsPreviewAvailable))
                        {
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            // Request the native attachment
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            ("Requesting Native Attachment File Data " + attachment.NativeFileName).AsBanner(light, true, false);
                            byte[] fileBytes = ApiHelper.RequestAttachment(httpClient: httpClient,
                                                   attachmentsUri: apiConfig.AttachmentsUri(messageId: retrievedMessage.Id, attachmentId: attachment.Id),
                                                   mimeType: "application/octet-stream");
                            attachment.NativeFileBytes = fileBytes;
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            // Save the file bytes as per the nativeFileName metadata field
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            if (fileBytes.Length > 0)
                            {
                                string fileName = Path.Combine(messageOutputPath, localMessageDirectory, attachment.NativeFileName);
                                (" √ saving file as " + fileName).AsClosingBanner(light);
                                using (var stream = new FileStream(fileName, FileMode.Create))
                                    stream.Write(attachment.NativeFileBytes, 0, attachment.NativeFileBytes.Length);
                            }
                            else
                            {
                                (" There was a problem retrieving attachment").AsClosingBanner(light);
                            }
                        }
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        // Request the pdf converted attachment then save it to disk.
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        foreach (var attachment in intakeMessage.Attachments.OrderBy(a => a.Id))
                        {
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            // Request the native attachment
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            ("Requesting PDF Converted Attachment File Data " + attachment.PdfFileName).AsBanner(light, true, false);
                            byte[] fileBytes = ApiHelper.RequestAttachment(httpClient: httpClient,
                                                   attachmentsUri: apiConfig.AttachmentsUri(messageId: retrievedMessage.Id, attachmentId: attachment.Id),
                                                   mimeType: "application/pdf");
                            attachment.PdfFileBytes = fileBytes;
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            // Save the file bytes as per the nativeFileName metadata field
                            // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                            if (fileBytes.Length > 0)
                            {
                                string fileName = Path.Combine(messageOutputPath, localMessageDirectory, attachment.PdfFileName);
                                (" √ saving file as " + fileName).AsClosingBanner(light);
                                using (var stream = new FileStream(fileName, FileMode.Create))
                                    stream.Write(attachment.PdfFileBytes, 0, attachment.PdfFileBytes.Length);
                            }
                            else
                            {
                                (" X looks like the attachment id " + attachment.Id + " didn't convert to pdf")
                                    .ToConsole(ConsoleColor.Red);
                                (" There was a problem retrieving converted attachment").AsClosingBanner(light);
                            }
                        }
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        // Send a message read event
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        "Sending Message Read Event".AsBanner(ConsoleColor.DarkGreen, true, false);
                        ApiHelper.RequesetMessageReadEvent(httpClient: httpClient,
                                    messageReadEventUri: apiConfig.MessageReadEventUri(retrievedMessage.Id),
                                    messageId: retrievedMessage.Id,
                                    subject: intakeMessage.Subject
                                  );
                        ConsoleHelper.HeaderLine(false);
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --



                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        // Send a attachmenet read event for each attachment
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                        foreach (var attachment in intakeMessage.Attachments)
                        {
                            ("Sending Attachment Read Event for Attachment " + attachment.NativeFileName).AsBanner(ConsoleColor.DarkGreen, true, false);
                            ApiHelper.RequestAttachmentReadEvent(httpClient: httpClient,
                                        attachmentReadUri: apiConfig.AttachmentReadUri(messageId: retrievedMessage.Id, attachmentId: attachment.Id)
                                      );
                            ConsoleHelper.HeaderLine(false);
                        }
                        // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --
                    }

                    if (intakeMessages.Any())
                        ConsoleHelper.HeaderLine(true);
                    else
                    {
                        ConsoleHelper.HeaderLine(light);
                        "No messages found - waiting 10 seconds ... (ctrl+c to quit)".AsBanner(ConsoleColor.DarkYellow, true);
                        Console.WriteLine();
                        System.Threading.Thread.Sleep(10000);
                    }
                }
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
