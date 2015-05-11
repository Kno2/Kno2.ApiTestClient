using System;
using System.Linq;
using System.Net.Http;
using Kno2.ApiTestClient.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kno2.ApiTestClient.Extensions
{
    public static class HttpClientExensions
    {
        public async static void CheckStatus(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var errorColor = ConsoleColor.Red;
                string.Format("{0} {1}", httpResponseMessage.StatusCode, httpResponseMessage.ReasonPhrase).AsOpeningBanner(errorColor);
                string.Format("{0} {1}", httpResponseMessage.RequestMessage.Method, httpResponseMessage.RequestMessage.RequestUri).ToConsole(ConsoleColor.Red);

                if (httpResponseMessage.RequestMessage.Content != null)
                {
                    foreach (var header in httpResponseMessage.RequestMessage.Content.Headers)
                    {
                        string.Format(" - {0}: {1}", header.Key, string.Join(",", header.Value)).ToConsole(errorColor);
                    }
                }
                string messageBody = await httpResponseMessage.Content.ReadAsStringAsync();
                
                //Write the body if we have one
                if (!string.IsNullOrWhiteSpace(messageBody))
                {
                    if (httpResponseMessage.RequestMessage.Content != null && httpResponseMessage.Content.Headers.ContentType.MediaType.Contains("application/json"))
                    {
                        JObject jToken = JObject.Parse(messageBody);
                        foreach (var token in jToken)
                        {
                            string.Format(" - {0}: {1}", token.Key, token.Value).ToConsole(errorColor);
                        }
                    }
                    else
                    {
                        //just output it to the screen
                        string.Format("Response: {0}", messageBody).ToConsole(errorColor);
                    }
                }

                string.Empty.AsClosingBanner(errorColor);

                Environment.Exit(-1);
            }
        }
    }
}