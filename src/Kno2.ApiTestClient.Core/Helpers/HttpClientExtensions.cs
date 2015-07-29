using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Kno2.ApiTestClient.Core.Resources;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class TokenHelpers
    {
        private static ConsoleColor _refreshTokenConsoleColor = ConsoleColor.DarkYellow;

        public static IToken GetRefreshToken(MediaType defaultMediaType, string tokenFile = "access-token.json")
        {
            var path = GetTokenPath(tokenFile);

            return File.Exists(path)
                ? ApiHelper.Deserialize<AuthResponse>(File.ReadAllText(path), defaultMediaType) 
                : new AuthResponse();
        }

        public static void SetTokens(this IToken authResponse, MediaType defaultMediaType, string tokenFile = "access-token.json")
        {
            var path = GetTokenPath(tokenFile);

            ConsoleHelper.SmallHeaderLine(_refreshTokenConsoleColor);
            ("   Saving Token to " + path).ToConsole(_refreshTokenConsoleColor);
            ConsoleHelper.SmallHeaderLine(_refreshTokenConsoleColor);

            ("   √ saving access token » " + authResponse.AccessToken.Substring(0, 15) + " ...").ToConsole(_refreshTokenConsoleColor);
            ("   √ saving refresh token » " + authResponse.RefreshToken).ToConsole(_refreshTokenConsoleColor);
            ("   √ saving expires » " + authResponse.Expires).ToConsole(_refreshTokenConsoleColor);

            ConsoleHelper.SmallHeaderLine(_refreshTokenConsoleColor);

            File.WriteAllText(path, ApiHelper.Serialize(authResponse, defaultMediaType));
        }

        private static string GetTokenPath(string tokenFile)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(directoryName, tokenFile);
            return path;
        }
    }

    public static class HttpClientExtensions
    {
        public static MediaType DefaultMediaType(this HttpClient source)
        {
            return source.DefaultRequestHeaders.DefaultMediaType();
        }

        public static MediaType DefaultMediaType(this HttpRequestHeaders source)
        {
            if (source.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
                return MediaType.json;
            if (source.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/xml")))
                return MediaType.xml;

            return MediaType.unknown;
        }
    }

}