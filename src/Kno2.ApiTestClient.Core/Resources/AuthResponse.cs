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
        TokenRequestType TokenRequestType { get; }
        string Error { get; set; }
    }

    public class AuthResponse : IToken
    {
        [JsonProperty("error")]
        public string Error { get; set; }

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

        [JsonIgnore]
        public DateTime RefreshTokenExpires
        {
            get { return Issued.AddSeconds(1800); }
        }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// New / null objects need to start with a secret based access token request
        /// </summary>
        [JsonIgnore]
        public TokenRequestType TokenRequestType
        {
            get
            {
                if (Expires == new DateTime())
                {
                    string.Format(" √ No access token has been created / saved").ToConsole(ConsoleColor.DarkGreen);
                    return TokenRequestType.ClientCredential;
                }

                DateTime utcNow = DateTime.UtcNow.AddSeconds(5);
                // our access token is expired but we need to determine what kind of 
                //  access token request to use.
                if (AccessTokenExpired(utcNow))
                {
                    string.Format(" √ Access token has expired: Expires {0} < Now {1}", utcNow, Expires).ToConsole(ConsoleColor.DarkRed);
                    // If our refresh token has also expired then we need to go to 
                    //  the primary token request for a new token and new refresh token
                    if (RefreshTokenExpired(utcNow))
                    {
                        string.Format(" √ Refresh token has also expired: Expires {0} < Now {1}", utcNow, RefreshTokenExpires).ToConsole(ConsoleColor.DarkRed);
                        return TokenRequestType.ClientCredential;
                    }

                    string.Format(" √ Refresh token is still valid for {0} seconds", (RefreshTokenExpires - utcNow).TotalSeconds).ToConsole(ConsoleColor.DarkYellow);
                    return TokenRequestType.RefreshToken;
                }

                // access token is still in the window so use the current access token
                string.Format(" √ Access token is still valid for {0} seconds", (Expires - utcNow).TotalSeconds).ToConsole(ConsoleColor.DarkYellow);
                return TokenRequestType.None;
            }
        }

        private bool AccessTokenExpired(DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(AccessToken)) return true;

            bool expired = (Expires < utcNow);

            return expired;
        }

        private bool RefreshTokenExpired(DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(RefreshToken)) return true;

            bool expired = (RefreshTokenExpires < utcNow);

            return expired;
        }
    }
}