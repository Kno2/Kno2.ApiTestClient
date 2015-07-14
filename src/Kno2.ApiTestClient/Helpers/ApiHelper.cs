using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Kno2.ApiTestClient.Extensions;
using Kno2.ApiTestClient.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Kno2.ApiTestClient.Helpers
{
    internal class ApiHelper
    {
        /// <summary>
        /// Creates a http client reqeust using a simple c# anonymous object that is serialized into
        /// a string content object and sent to a API endpoint expecting a application/json media type
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="messageUri">The specific API endpoint for making draft id requests</param>
        /// <returns></returns>
        public static MessageResource RequestMessageDraft(HttpClient httpClient, Uri messageUri)
        {
            // Make a POST request to the draft id endpoint.  It will return a draft id response as a bare string.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.PutAsync(messageUri, null).Result;
            result.CheckStatus();
            string responseJson = result.Content.ReadAsStringAsync().Result;
            WriteTimingOutput("making draft id request against", messageUri, stopwatch.ElapsedMilliseconds);

            return Deserialize<MessageResource>(responseJson, httpClient.DefaultMediaType());
        }

        /// <summary>
        /// Creates a http request to get the available document types that the current users has access too
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="documentTypesUri">The specific API endpoint for making document types requests</param>
        /// <returns></returns>
        public static IEnumerable<string> RequestDocumentTypes(HttpClient httpClient, Uri documentTypesUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.GetAsync(documentTypesUri).Result;
            result.CheckStatus();
            string responseJson = result.Content.ReadAsStringAsync().Result;
            WriteTimingOutput("making draft id request against", result.RequestMessage.RequestUri, stopwatch.ElapsedMilliseconds);


            var documentTypesResource = Deserialize<DocumentTypesResource>(responseJson, httpClient.DefaultMediaType());

            (" √ parsing response - document types found » " + documentTypesResource.DocumentTypes.Count()).ToConsole();

            return documentTypesResource.DocumentTypes.Select(x => x.Name);
        }

        public static AttachmentResource UploadAttachment(HttpClient httpClient, Uri attachmentsUri, string fileName, AttachmentResource attachment)
        {
            (" √ creating attachment metadata for file » " + attachment.NativeFileName).ToConsole();
            ("   + confidentiality » " + attachment.AttachmentMeta.Confidentiality).ToConsole();
            ("   + documentType » " + attachment.AttachmentMeta.DocumentType).ToConsole();
            ("   + documentDate » " + attachment.AttachmentMeta.DocumentDate).ToConsole();
            ("   + documentDescription » " + attachment.AttachmentMeta.DocumentDescription).ToConsole();
            


            string serializeObject = Serialize<AttachmentMetaResource>(attachment.AttachmentMeta, httpClient.DefaultMediaType());
            (" √ serializing request object to " + httpClient.DefaultMediaType()).ToConsole();
            


            // This API requires a POST of both text based and binary data using MultipartContent
            //  https://msdn.microsoft.com/System.Net.Http.MultipartContent
            var multipartContent = new MultipartFormDataContent();
            


            // Using the StringContent (https://msdn.microsoft.com/System.Net.Http.StringContent) class to encode
            //  and setup the required mime type for this endpoint
            var contentString = new StringContent(serializeObject, Encoding.UTF8, httpClient.DefaultMediaType().Description());
            string.Format(" √ creating request content (string) object using as {0}", httpClient.DefaultMediaType().Description()).ToConsole();



            // Using the ByteArrayContent (https://msdn.microsoft.com/System.Net.Http.ByteArrayContent) class to encode
            //  and setup the required array buffer
            ByteArrayContent byteArrayContent = new ByteArrayContent(attachment.NativeFileBytes, 0, attachment.NativeFileBytes.Length);



            // Add the two content httpcontent based instances to the collection to be sent up to the API
            multipartContent.Add(contentString);
            multipartContent.Add(byteArrayContent, fileName, fileName);



            // Make a POST request to the draft id endpoint.  It will return a draft id response as a bare string.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.PostAsync(attachmentsUri, multipartContent).Result;
            result.CheckStatus();
            string responseJson = result.Content.ReadAsStringAsync().Result;
            WriteTimingOutput("making attachment upload request against", attachmentsUri, stopwatch.ElapsedMilliseconds);


            var attachmentResource = Deserialize<AttachmentResource>(responseJson, httpClient.DefaultMediaType());


            (" √ sent " + attachment.NativeFileBytes.Length + " bytes To API").ToConsole();

            return attachmentResource;
        }

        /// <summary>
        /// Creates a http request that will upload a file binary to an existing MessageResource draft.  The payload
        /// also includes information about the attachment or metadata
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="attachmentsUri"></param>
        /// <param name="draftId"></param>
        /// <param name="confidentiality"></param>
        /// <param name="documentType"></param>
        /// <param name="documentDate"></param>
        /// <param name="documentDescription"></param>
        /// <param name="attachmentBytes"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static long UploadAttachment(HttpClient httpClient, Uri attachmentsUri, long draftId, Confidentiality confidentiality, string documentType,
            DateTime documentDate, string documentDescription, byte[] attachmentBytes, string fileName)
        {
            // This is an example of using a specific Uri resource.
            Uri attachmentResource = new Uri(string.Format("{0}/{1}", attachmentsUri, draftId), UriKind.Relative);

            // Creating anonymous object that represents the miminum required fields for the attachment metadata
            //  Confidentiality required a 0 - Normal, 1 - Restricted or 3 - Very Restricted
            var attachmentMetadata = new
            {
                confidentiality = (Int32)confidentiality,
                documentType = documentType,
                documentDate = documentDate,
                documentDescription = documentDescription
            };
            (" √ creating attachment metadata for file » " + fileName).ToConsole();
            ("   + confidentiality » " + confidentiality).ToConsole();
            ("   + documentType » " + documentType).ToConsole();
            ("   + documentDate » " + documentDate).ToConsole();
            ("   + documentDescription » " + documentDescription).ToConsole();



            // Using Json.Net (http://www.nuget.org/packages/Newtonsoft.Json/) we serialize the object into
            //  a json string
            string serializeObject = JsonConvert.SerializeObject(attachmentMetadata);
            (" √ serializing request object to json").ToConsole();



            // This API requires a POST of both text based and binary data using MultipartContent
            //  https://msdn.microsoft.com/System.Net.Http.MultipartContent
            var multipartContent = new MultipartFormDataContent();



            // Using the StringContent (https://msdn.microsoft.com/System.Net.Http.StringContent) class to encode
            //  and setup the required mime type for this endpoint
            var contentString = new StringContent(serializeObject, Encoding.UTF8, "application/json");
            (" √ creating request content (string) object using json, UTF-8 encoding and\r\n     application/json media type").ToConsole();



            // Using the ByteArrayContent (https://msdn.microsoft.com/System.Net.Http.ByteArrayContent) class to encode
            //  and setup the required array buffer
            ByteArrayContent byteArrayContent = new ByteArrayContent(attachmentBytes, 0, attachmentBytes.Length);



            // Add the two content httpcontent based instances to the collection to be sent up to the API
            multipartContent.Add(contentString);
            multipartContent.Add(byteArrayContent, fileName, fileName);



            // Make a POST request to the draft id endpoint.  It will return a draft id response as a bare string.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.PostAsync(attachmentResource, multipartContent).Result;
            result.CheckStatus();
            string responseJson = result.Content.ReadAsStringAsync().Result;
            WriteTimingOutput("making attachment upload request against", attachmentsUri, stopwatch.ElapsedMilliseconds);

            (" √ sent " + attachmentBytes.Length + " bytes To API").ToConsole();

            // Parse the metadata response to extract the attachment id
            JToken jToken = JObject.Parse(responseJson);
            long attachmentId = Convert.ToInt64(jToken.SelectToken("attachmentId").Value<string>());

            return attachmentId;
        }

        /// <summary>
        /// Creates an http request that will request attachment metadata for a specific attachment.
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="attachmentsUri"></param>
        /// <returns>application/json</returns>
        public static AttachmentResource RequestAttachmentMetadata(HttpClient httpClient, Uri attachmentsUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.GetAsync(attachmentsUri).Result;
            result.CheckStatus();
            var fileBytes = result.Content.ReadAsStringAsync().Result;
            WriteTimingOutput("making attachment request against", attachmentsUri, stopwatch.ElapsedMilliseconds);

            (" √ received " + fileBytes.Length + " bytes from API").ToConsole();

            var attachmentResource = Deserialize<AttachmentResource>(fileBytes, httpClient.DefaultMediaType());

            return attachmentResource;
        }

        /// <summary>
        /// Creates an http request that will request nateive attachment binary for a specific attachment.
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="attachmentsUri"></param>
        /// <returns>byte[]</returns>
        public static byte[] RequestAttachment(HttpClient httpClient, Uri attachmentsUri, string mimeType)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, attachmentsUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mimeType));


            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.SendAsync(request).Result;
            result.CheckStatus();
            var fileBytes = result.Content.ReadAsByteArrayAsync().Result;
            WriteTimingOutput("making attachment request against", attachmentsUri, stopwatch.ElapsedMilliseconds);

            (" √ received " + fileBytes.Length + " bytes from API").ToConsole();

            return fileBytes;
        }

        /// <summary>
        /// An example of a intake query request
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="documentsMessagesUri"></param>
        /// <returns></returns>
        public static IEnumerable<MessageResource> RequestUnprocessedIntakeMessages(HttpClient httpClient, Uri documentsMessagesUri)
        {
            IEnumerable<MessageResource> messageResources = Enumerable.Empty<MessageResource>();

            // This example is going to parse the Uri to display the query paramters.
            Uri resource = new Uri(httpClient.BaseAddress, documentsMessagesUri);



            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.GetAsync(documentsMessagesUri).Result;
            result.CheckStatus();
            string responseJson = result.Content.ReadAsStringAsync().Result;
            WriteTimingOutput("making unprocessed intake messages request against", resource, stopwatch.ElapsedMilliseconds);
            NameValueCollection queryString = resource.ParseQueryString();
            foreach (string key in queryString)
                ("   - " + key + ": " + queryString[key]).ToConsole();



            // parse the response.items for the messages and convert them to message resources
            JToken jToken = JObject.Parse(responseJson);
            if (string.IsNullOrWhiteSpace(responseJson))
                return messageResources;

            var messages = jToken.SelectToken("items").ToString();
            if (string.IsNullOrWhiteSpace(messages))
                return messageResources;

            messageResources = Deserialize<IEnumerable<MessageResource>>(messages, httpClient.DefaultMediaType());

            return messageResources;
        }

        /// <summary>
        /// Send draft performs the function of creating a MessageResource in a unsent state
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="messageUri"></param>
        /// <param name="messageResource"></param>
        public static void SendDraft(HttpClient httpClient, Uri messageUri, MessageResource messageResource)
        {
            string serializeObject = Serialize(messageResource, httpClient.DefaultMediaType());
            (" √ serializing request object to " + httpClient.DefaultMediaType()).ToConsole();

            HttpResponseMessage result = httpClient.PutAsync(messageUri, new StringContent(serializeObject, Encoding.UTF8, httpClient.DefaultMediaType().Description())).Result;
            result.CheckStatus();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="messageSendUri"></param>
        /// <param name="messageResource"></param>
        public static void SendRelease(HttpClient httpClient, Uri messageSendUri, MessageResource messageResource)
        {
            string serializeObject = Serialize(messageResource, httpClient.DefaultMediaType());
            (" √ serializing request object to " + httpClient.DefaultMediaType()).ToConsole();

            HttpResponseMessage result = httpClient.PostAsync(messageSendUri, new StringContent(serializeObject, Encoding.UTF8, httpClient.DefaultMediaType().Description())).Result;
            result.CheckStatus();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient">Existing http client object setup with auth headers</param>
        /// <param name="directoryValidateUri"></param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public static Dictionary<string, bool> ValidateAddresses(HttpClient httpClient, Uri directoryValidateUri, params string[] addresses)
        {
            // Since this GET request takes a set of direct MessageResource addresses as the url parameters 
            //  we are using FormUrlEncodedContent to create the url query parameter
            var content = new FormUrlEncodedContent(addresses.Select(x => new KeyValuePair<string, string>("addresses", x)));
            string queryParameters = content.ReadAsStringAsync().Result;


            // Build up the address validation collection
            UriBuilder uriBuilder = new UriBuilder(httpClient.BaseAddress);
            uriBuilder.Path = directoryValidateUri.ToString();
            uriBuilder.Query = queryParameters;

            HttpResponseMessage result = httpClient.GetAsync(uriBuilder.Uri).Result;
            result.CheckStatus();
            string responseJson = result.Content.ReadAsStringAsync().Result;

            JObject jObject = JObject.Parse(responseJson);
            var addressValidationResults = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, JToken> validationResultItem in jObject)
            {
                bool isValid = Convert.ToBoolean(validationResultItem.Value);
                addressValidationResults.Add(validationResultItem.Key, isValid);
                if (isValid)
                    string.Format(" √ {0} is valid", validationResultItem.Key).ToConsole();
                else
                    string.Format(" X {0} is not valid", validationResultItem.Key).ToConsole(ConsoleColor.DarkRed);
            }


            return addressValidationResults;
        }

        /// <summary>
        /// Downloads the the selected MessageResource body
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="messageUri"></param>
        /// <returns></returns>
        public static string RequestMessage(HttpClient httpClient, Uri messageUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.GetAsync(messageUri).Result;
            result.CheckStatus();
            WriteTimingOutput("making document request MessageResource request against", messageUri, stopwatch.ElapsedMilliseconds);
            string responseJson = result.Content.ReadAsStringAsync().Result;

            return responseJson;
        }

        /// <summary>
        /// Send a MessageResource 'read' event to the server
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="messageReadEventUri"></param>
        /// <param name="messageId"></param>
        /// <param name="subject"></param>
        public static void RequesetMessageReadEvent(HttpClient httpClient, Uri messageReadEventUri, string messageId, string subject)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, messageReadEventUri);

            //create an anonymous object that will be serialized as json
            var request = new
            {
                isProcessed = true,
                processType = "emrexported"
            };

            // Using Json.Net (http://www.nuget.org/packages/Newtonsoft.Json/) we serialize the object into
            //  a json string
            string serializeObject = JsonConvert.SerializeObject(request, Formatting.Indented);

            var messageProcessedContent = new StringContent(serializeObject, Encoding.UTF8, "application/json");
            httpRequestMessage.Content = messageProcessedContent;


            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            HttpResponseMessage result = httpClient.SendAsync(httpRequestMessage).Result;
            result.CheckStatus();
            WriteTimingOutput("making attachment read event request against", messageReadEventUri, stopwatch.ElapsedMilliseconds);
        }

        public static void RequestAttachmentReadEvent(HttpClient httpClient, Uri attachmentReadUri)
        {
            // Make a GET request to the document request endpoint.  It will return a collection of document types.
            //  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            HttpResponseMessage result = httpClient.PutAsync(attachmentReadUri, null).Result;
            result.CheckStatus();
            WriteTimingOutput("making attachment read event request against", attachmentReadUri, stopwatch.ElapsedMilliseconds);
        }

        private static void WriteTimingOutput(string message, Uri path, long milliseconds)
        {
            (" √ " + message).ToConsole();
            ("  + Request Url:  " + path.AbsolutePath).ToConsole();
            ("  + Request Time: " + milliseconds + " milliseconds").ToConsole();
        }

        public static string Serialize<T>(T value, MediaType mediaType)
        {
            if (mediaType == MediaType.xml)
            {
                Encoding enc = Encoding.UTF8;

                using (var ms = new MemoryStream())
                {
                    var xmlWriterSettings = new XmlWriterSettings
                    {
                        CloseOutput = false,
                        Encoding = enc,
                        OmitXmlDeclaration = false,
                        Indent = true
                    };
                    using (XmlWriter xmlWriter = XmlWriter.Create(ms, xmlWriterSettings))
                    {
                        var s = new XmlSerializer(typeof(T));
                        s.Serialize(xmlWriter, value);
                    }

                    return enc.GetString(ms.ToArray());
                }
            }

            // Using Json.Net (http://www.nuget.org/packages/Newtonsoft.Json/) we serialize the object into
            //  a json string
            if (mediaType == MediaType.json)
            {
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                return JsonConvert.SerializeObject(value, jsonSerializerSettings);
            }

            throw new SerializationException("no serializer for " + mediaType);
        }

        public static T Deserialize<T>(string rawValue, MediaType mediaType)
        {
            if (mediaType == MediaType.xml)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                StringReader reader = new StringReader(rawValue);

                T value = (T)xmlSerializer.Deserialize(reader);
                return value;
            }

            if (mediaType == MediaType.json)
            {
                return JsonConvert.DeserializeObject<T>(rawValue);
            }

            throw new SerializationException("no serializer for " + mediaType);
        }
    }
}
