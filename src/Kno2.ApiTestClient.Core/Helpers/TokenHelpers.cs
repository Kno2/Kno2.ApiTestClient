using System;
using System.IO;
using System.Reflection;
using Kno2.ApiTestClient.Core.Resources;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class TokenHelpers
    {
        private const ConsoleColor RefreshTokenConsoleColor = ConsoleColor.DarkYellow;

        public static IToken GetRefreshToken(MediaType defaultMediaType = MediaType.json, string tokenFile = "access-token.json")
        {
            string path = tokenFile.AsAppPath(appNamePrefix: false);

            return File.Exists(path)
                ? ApiHelper.Deserialize<AuthResponse>(File.ReadAllText(path), defaultMediaType) 
                : new AuthResponse();
        }

        public static void Save(this IToken authResponse, MediaType defaultMediaType = MediaType.json, string tokenFile = "access-token.json")
        {
            string path = tokenFile.AsAppPath(appNamePrefix: false);

            ConsoleHelper.SmallHeaderLine(RefreshTokenConsoleColor);
            ("   Saving Token to " + path).ToConsole(RefreshTokenConsoleColor);
            ConsoleHelper.SmallHeaderLine(RefreshTokenConsoleColor);

            ("   √ saving access token » " + authResponse.AccessToken.Substring(0, 15) + " ...").ToConsole(RefreshTokenConsoleColor);
            ("   √ saving refresh token » " + authResponse.RefreshToken).ToConsole(RefreshTokenConsoleColor);
            ("   √ saving expires » " + authResponse.Expires).ToConsole(RefreshTokenConsoleColor);

            ConsoleHelper.SmallHeaderLine(RefreshTokenConsoleColor);

            File.WriteAllText(path, ApiHelper.Serialize(authResponse, defaultMediaType));
        }
    }
}