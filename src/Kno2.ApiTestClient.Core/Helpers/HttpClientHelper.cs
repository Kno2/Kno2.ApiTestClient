using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Kno2.ApiTestClient.Core.Extensions;
using Kno2.ApiTestClient.Core.MessageHandlers;
using Kno2.ApiTestClient.Core.Resources;
using Newtonsoft.Json;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class HttpClientHelper
    {
        /// <summary>
        /// Creates a configured http client that can be used for the lifetime of the application
        /// </summary>
        /// <param name="baseUri">Api base uri that all requests will come from</param>
        /// <param name="clientId">Security field akin to username that is associated with the user</param>
        /// <param name="clientSecret">Security field akin to password that is associated with the user</param>
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


            var accessTokenHandler = new AccessTokenHandler(new Uri(baseUri, authUri), clientId, clientSecret, appId);
            HttpClient httpClient = HttpClientFactory.Create(new LogHandler(), accessTokenHandler);
            httpClient.BaseAddress = baseUri;
            //var httpClient = new HttpClient(refreshTokenHandler) { BaseAddress = baseUri };
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

            // Setup the authorization grant type - https://tools.ietf.org/html/rfc6749#appendix-A.10
            var authValues = new List<KeyValuePair<string, string>>();
            authValues.Add(new KeyValuePair<string, string>("grant_type", grantType));
            (" √ setting grant type » " + grantType).ToConsole();



            if (grantType == "client_credentials")
            {
                // The initial authorization request is make against the token endpoint.
                // Using a grant type of client_credentials instead of password we're afforded
                //  decoupling of security credentials from the account username and password.
                //authValues.Add(new KeyValuePair<string, string>("client_id", clientId));
                //authValues.Add(new KeyValuePair<string, string>("client_secret", clientSecret));

                //(" √ setting client id » " + clientId).ToConsole();
                //(" √ setting client secret » " + clientSecret).ToConsole();
            }

            if (grantType == "password")
            {
                // An alternate and less desired authorization request is using username and password.
                authValues.Add(new KeyValuePair<string, string>("username", clientId));
                authValues.Add(new KeyValuePair<string, string>("password", clientSecret));

                (" √ setting username » " + clientId).ToConsole();
                (" √ setting password » " + clientSecret).ToConsole();
            }

            FormUrlEncodedContent content = new FormUrlEncodedContent(authValues);

            // Make a POST request to the auth endpoint. It will return a token response
            //  from the server.  (example is also showing some simple timing diagnostics)
            var stopwatch = new Stopwatch(); stopwatch.Start();
            HttpResponseMessage result = httpClient.PostAsync(authUri, content).Result;
            HttpClientExensions.CheckStatus(result);
            string responseJson = result.Content.ReadAsStringAsync().Result;
            (" √ authenticating against » " + authUri + " (" + stopwatch.ElapsedMilliseconds + " ms)").ToConsole();



            // Taking the json response we're simply deserializing the json string into a c# object
            //  Doing full serializing with a class is not required and other types of grant_types 
            //   may return other fields that aren't present in the AuthResponse sample class
            var authResponse = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
            (" √ authorization response received" + authUri).ToConsole();



            //   // With the auth response captured we assign a pernament auth header configuration
            //   //  against the http client so that each request will have that header present.
            //   // This is a just a example as you can also append the auth header directly to http messages themselves.
            //   httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
            //   (" √ setting auth token to http client headers » " + authResponse.AccessToken.Substring(0, 15) + " ...").ToConsole();



            // Persit the tokens
            authResponse.Save(httpClient.DefaultMediaType());



            return httpClient;
        }
    }
}