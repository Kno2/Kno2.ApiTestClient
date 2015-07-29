using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kno2.ApiTestClient.Core.Helpers;
using Kno2.ApiTestClient.Core.Resources;
using Newtonsoft.Json;

namespace Kno2.ApiTestClient.Core.MessageHandlers
{
    public class AccessTokenHandler : DelegatingHandler
    {
        private readonly Uri _authUri;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appId;

        public AccessTokenHandler(Uri authUri, string clientId, string clientSecret, string appId)
        {
            _authUri = authUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _appId = appId;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
           IToken token = TokenHelpers.GetRefreshToken();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _authUri);
            httpRequestMessage.Headers.Add("appid", _appId);
            HttpResponseMessage httpResponseMessage;

            switch (token.TokenRequestType)
            {
                case TokenRequestType.None:
                    break;

                case TokenRequestType.ClientCredential:

                    httpResponseMessage = await PrimaryTokenRequest(httpRequestMessage, cancellationToken);
                    token = JsonConvert.DeserializeObject<AuthResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
                    token.Save();
                    break;

                case TokenRequestType.RefreshToken:

                    httpResponseMessage = await RefreshTokenRequest(httpRequestMessage, token.RefreshToken, cancellationToken);
                    string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

                    // The refresh token request failed - try the primary token
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        httpResponseMessage = await PrimaryTokenRequest(httpRequestMessage, cancellationToken);
                        token = JsonConvert.DeserializeObject<AuthResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
                    }

                    token.Save();
                    break;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            return response;
        }

        private async Task<HttpResponseMessage> PrimaryTokenRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _clientId, _clientSecret))));

            request.WriteApiEntry(nameValueCollection);
            HttpResponseMessage httpResponseMessage = await base.SendAsync(request, cancellationToken);
            return httpResponseMessage;
        }

        private async Task<HttpResponseMessage> RefreshTokenRequest(HttpRequestMessage request, string refreshToken, CancellationToken cancellationToken)
        {
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };
            request.Content = new FormUrlEncodedContent(nameValueCollection);

            request.WriteApiEntry(nameValueCollection);
            HttpResponseMessage httpResponseMessage = await base.SendAsync(request, cancellationToken);
            return httpResponseMessage;
        }
    }
}