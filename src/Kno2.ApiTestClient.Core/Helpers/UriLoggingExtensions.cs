using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class UriLoggingExtensions
    {
        public static async void WriteApiEntry(this HttpRequestMessage source)
        {
            using (var output = File.AppendText(("api.log").AsAppPath()))
            {
                await output.WriteLineAsync(string.Format("{0} {1}", source.Method.Method, source.RequestUri.PathAndQuery));
            }
        }

        public static async void WriteApiEntry(this Uri source, List<KeyValuePair<string, string>> nameValueCollection)
        {
            var stringBuilder = new StringBuilder();
            foreach (var keyValuePair in nameValueCollection)
            {
                stringBuilder.Append(" " + keyValuePair.Key + ":" + keyValuePair.Value);
            }

            using (var output = File.AppendText(("api.log").AsAppPath()))
            {
                var value = source.PathAndQuery + " " + stringBuilder;
                await output.WriteLineAsync(value.Trim());
            }
        }

        public static async void WriteApiEntry(this HttpRequestMessage source, List<KeyValuePair<string, string>> nameValueCollection)
        {
            var stringBuilder = new StringBuilder();
            foreach (var keyValuePair in nameValueCollection)
            {
                stringBuilder.Append(" " + keyValuePair.Key + ":" + keyValuePair.Value);
            }

            using (var output = File.AppendText(("api.log").AsAppPath()))
            {
                var value = string.Format("{0} {1} {2}", source.Method.Method, source.RequestUri.PathAndQuery, stringBuilder);
                await output.WriteLineAsync(value.Trim());
            }
        }
    }
}