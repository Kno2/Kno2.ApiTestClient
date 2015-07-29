using System;
using Kno2.ApiTestClient.Core.Helpers;
using Newtonsoft.Json;

namespace Kno2.ApiTestClient.Core.Resources
{
    public interface IToken
    {
        DateTime Expires { get; set; }
        string RefreshToken { get; set; }
        string AccessToken { get; set; }
        bool NeedsNewRefreshToken();
        bool HasAuthToken();
        bool AccessTokenExpired();
        bool HasRefreshToken();
        RequestType RequestType { get; }
    }

    public class AuthResponse : IToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(".issued")]
        public DateTime Issued { get; set; }

        [JsonProperty(".expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        public bool HasAuthToken()
        {
            return !string.IsNullOrWhiteSpace(AccessToken);
        }

        public bool NeedsNewRefreshToken()
        {
            if (this == new AuthResponse()) return false;

            if (string.IsNullOrWhiteSpace(RefreshToken)) return false;

            if (AccessTokenExpired()) return true;

            return false;
        }

        public bool AccessTokenExpired()
        {
            if (this == new AuthResponse()) return true;

            return (Expires < DateTime.UtcNow);
        }

        public bool RefreshTokenExpired()
        {
            if (this == new AuthResponse()) return true;

            return (Issued.AddSeconds(3600) < DateTime.UtcNow);
        }

        public bool HasRefreshToken()
        {
            return !string.IsNullOrWhiteSpace(RefreshToken);
        }

        /// <summary>
        /// New / null objects need to start with a secret based access token request
        /// </summary>
        public RequestType RequestType {
            get
            {
                if (this == new AuthResponse())
                    return RequestType.ClientCredential;

                // This object doesn't have a refresh token yet
                if (string.IsNullOrWhiteSpace(RefreshToken) || RefreshTokenExpired())
                    return RequestType.ClientCredential;

                // We have a refresh token and a expired access token
                if (Expires < DateTime.UtcNow)
                    return RequestType.RefreshToken;

                //Just use your current access token
                return RequestType.None;
            }
        }
    }
}