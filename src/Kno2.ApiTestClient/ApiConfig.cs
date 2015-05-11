using System;
using System.Configuration;
using Kno2.ApiTestClient.Helpers;

namespace Kno2.ApiTestClient
{
    /// <summary>
    /// This class shows an unsecured method of storing API authentication component values
    /// </summary>
    public class ApiConfig
    {
        public string ClientId;
        public string ClientSecret;
        public Uri BaseUri;
        public Uri AuthUri;

        private readonly Uri _messagesUriTemplate;
        private readonly Uri _attachmentsUriTemplate;
        private readonly Uri _messageSendUriTemplate;
        private readonly Uri _documentTypesUriTemplate;
        private readonly Uri _directoryValidateTemplate;
        private readonly Uri _messageReadEventUriTemplate;
        private readonly Uri _attachmentReadUriTemplate;

        public string DirectMessageDomain;
        public string EmrSessionValue;
        private readonly Uri _messageSearchTemplate;

        public ApiConfig()
        {
            ClientId = ConfigurationManager.AppSettings["ClientId"];
            ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            BaseUri = new Uri(ConfigurationManager.AppSettings["BaseUri"]);
            AuthUri = new Uri(ConfigurationManager.AppSettings["AuthUri"], UriKind.Relative);

            //try to create a direct address domain by convention
            string directMessageDomain = ConfigurationManager.AppSettings["DirectMessageDomain"];
            if (string.IsNullOrWhiteSpace(directMessageDomain) || directMessageDomain == "your_direct_message_domain")
            {
                string[] segments = BaseUri.Host.Split('.');
                if (segments.Length == 3)
                {
                    DirectMessageDomain = string.Format("{0}.direct.{1}.{2}", segments[0], segments[1], segments[2]);
                }
            }
            else
            {
                DirectMessageDomain = directMessageDomain;
            }

            EmrSessionValue = ConfigurationManager.AppSettings["EmrSessionValue"];

            _messagesUriTemplate = new Uri(ConfigurationManager.AppSettings["MessagesUriTemplate"], UriKind.Relative);
            _attachmentsUriTemplate = new Uri(ConfigurationManager.AppSettings["AttachmentsUriTemplate"], UriKind.Relative);
            _messageSendUriTemplate = new Uri(ConfigurationManager.AppSettings["MessageSendUriTemplate"], UriKind.Relative);
            _messageReadEventUriTemplate = new Uri(ConfigurationManager.AppSettings["MessageReadEventUriTemplate"], UriKind.Relative);
            _documentTypesUriTemplate = new Uri(ConfigurationManager.AppSettings["DocumentTypesUriTemplate"], UriKind.Relative);
            _directoryValidateTemplate = new Uri(ConfigurationManager.AppSettings["DirectoryValidateTemplate"], UriKind.Relative);
            _messageSearchTemplate = new Uri(ConfigurationManager.AppSettings["MessageSearchTemplate"], UriKind.Relative);
            _attachmentReadUriTemplate = new Uri(ConfigurationManager.AppSettings["AttachmentReadUriTemplate"], UriKind.Relative);
            


            (" -- -- -- -- -- -- -- -- Setting Security -- -- -- -- -- -- -- --").ToConsole();
            (" √ setting client id » " + ClientId).ToConsole();
            (" √ setting client secret » " + ClientSecret).ToConsole();
            (" √ setting base Api address » " + BaseUri).ToConsole();
            (" √ setting direct message domain » " + DirectMessageDomain).ToConsole();
            if (!string.IsNullOrWhiteSpace(EmrSessionValue))
                (" √ setting emr session value » " + EmrSessionValue).ToConsole();

            (" -- -- -- -- -- -- -- -- Setting Templates -- -- -- -- -- -- -- --").ToConsole();
            (" √ setting message uri template » " + _messagesUriTemplate).ToConsole();
            (" √ setting attachment uri template » " + _attachmentsUriTemplate).ToConsole();
            (" √ setting message send uri template » " + _messageSendUriTemplate).ToConsole();
            (" √ setting message read event uri template » " + _messageReadEventUriTemplate).ToConsole();
            (" √ setting document types uri template » " + _documentTypesUriTemplate).ToConsole();
            (" √ setting directory address validate uri template » " + _directoryValidateTemplate).ToConsole();
            (" √ setting message search uri template » " + _messageSearchTemplate).ToConsole();
            (" √ setting attachment read uri template » " + _messageSearchTemplate).ToConsole();
        }

        public Uri AttachmentsUri(string messageId, string attachmentId = null)
        {
            var uriTemplate = new UriTemplate(_attachmentsUriTemplate.ToString());

            if (string.IsNullOrWhiteSpace(attachmentId))
                attachmentId = " ";

            return uriTemplate.BindByPosition(BaseUri, messageId, attachmentId).TrimUri();
        }

        public Uri MessagesUri(string messageId = null)
        {
            var uriTemplate = new UriTemplate(_messagesUriTemplate.ToString());

            if (string.IsNullOrWhiteSpace(messageId))
                messageId = " ";

            return uriTemplate.BindByPosition(BaseUri, messageId).TrimUri();
        }

        public Uri MessageSendUri(string messageId)
        {
            var uriTemplate = new UriTemplate(_messageSendUriTemplate.ToString());

            return uriTemplate.BindByPosition(BaseUri, messageId);
        }


        public Uri MessageReadEventUri(string messageId)
        {
            var uriTemplate = new UriTemplate(_messageReadEventUriTemplate.ToString());

            return uriTemplate.BindByPosition(BaseUri, messageId);
        }

        public Uri DocumentTypesUri()
        {
            return _documentTypesUriTemplate;
        }

        public Uri DirectoryValidate()
        {
            return _directoryValidateTemplate;
        }

        public Uri MessageSearch()
        {
            return _messageSearchTemplate;
        }

        public Uri AttachmentReadUri(string messageId, string attachmentId)
        {
            var uriTemplate = new UriTemplate(_attachmentReadUriTemplate.ToString());

            return uriTemplate.BindByPosition(BaseUri, messageId, attachmentId);
        }
    }

    internal static class UriExtensions
    {
        public static Uri TrimUri(this Uri source)
        {
            return new Uri(source.ToString().Trim());
        }
    }
}
