using System;
using System.Net.Http;
using Kno2.ApiTestClient.Core.MessageHandlers;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class HttpClientHelper
    {
        /// <summary>
        /// Creates a configured http client that can be used for the lifetime of the application
        /// </summary>
        /// <param name="baseUri">Api base uri that all requests will come from</param>
        /// <param name="defaultAccept">The default accept header to use for the client.</param>
        /// <param name="clientId">Security field akin to username that is associated with the user</param>
        /// <param name="clientSecret">Security field akin to password that is associated with the user</param>
        /// <param name="appId">The appId to use for the client</param>
        /// <param name="authUri">The specific API endpoint of the token service</param>
        /// <param name="grantType">Default grant type of the auth request (client_credentials)</param>
        /// <param name="emrSessionValue"></param>
        /// <returns></returns>
        public static HttpClient CreateHttpClient(Uri baseUri, string defaultAccept, string clientId, string clientSecret, string appId, Uri authUri,
            string grantType = "client_credentials", string emrSessionValue = null)
        {
            // Creating a Web Api HttpClient with an inital base address to use for all requests
            //  HttpClient lifetime is meant to exist for as long as http requests are needed.
            //  using HttpClient within a using() block is not advised


            // Create a Http Client with an message logging and token handler
            var accessTokenHandler = new AccessTokenHandler(new Uri(baseUri, authUri), clientId, clientSecret, appId);
            HttpClient httpClient = HttpClientFactory.Create(new MessageLoggingHandler(), accessTokenHandler);
            httpClient.BaseAddress = baseUri;
            (" √ http client created  » " + baseUri).ToConsole();



            // Add AppId header to all requests
            if (!string.IsNullOrWhiteSpace(appId))
                httpClient.DefaultRequestHeaders.Add("AppId", appId);



            // Add a simple header to help track the emr client with various api debugging tools
            if (string.IsNullOrWhiteSpace(emrSessionValue))
                emrSessionValue = Guid.NewGuid().ToString("N");
            httpClient.DefaultRequestHeaders.Add("emr-session", emrSessionValue);

            httpClient.DefaultRequestHeaders.Add("Accept", defaultAccept);

            return httpClient;
        }
    }
}