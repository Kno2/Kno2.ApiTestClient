using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kno2.ApiTestClient.Core.Extensions;
using Kno2.ApiTestClient.Core.Resources;
using Newtonsoft.Json;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public enum RequestType
    {
        None,
        Password,
        ClientCredential,
        RefreshToken
    }

    public class AccessTokenHandler : DelegatingHandler
    {
        private readonly Uri _authUri;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appId;
        private readonly string _tokenFile;

        public AccessTokenHandler(Uri authUri, string clientId, string clientSecret, string appId, string tokenFile = "access-token.json")
        {
            _authUri = authUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _appId = appId;
            _tokenFile = tokenFile;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            MediaType defaultMediaType = request.Headers.DefaultMediaType();

            IToken token = TokenHelpers.GetRefreshToken(defaultMediaType, _tokenFile);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _authUri);
            httpRequestMessage.Headers.Add("appid", _appId);
            HttpResponseMessage httpResponseMessage;

            switch (token.RequestType)
            {
                case RequestType.ClientCredential:

                    httpRequestMessage.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials")
                    });

                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _clientId, _clientSecret))));

                    httpResponseMessage = await base.SendAsync(httpRequestMessage, cancellationToken);
                    token = JsonConvert.DeserializeObject<AuthResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
                    token.SetTokens(defaultMediaType);
                    break;


                case RequestType.RefreshToken:

                    httpRequestMessage.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "refresh_token"),
                        new KeyValuePair<string, string>("client_id", _clientId),
                        new KeyValuePair<string, string>("refresh_token", token.RefreshToken)
                    });

                    httpResponseMessage = await base.SendAsync(httpRequestMessage, cancellationToken);
                    token = JsonConvert.DeserializeObject<AuthResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
                    token.SetTokens(defaultMediaType);
                    break;

                case RequestType.None:
                    break;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}