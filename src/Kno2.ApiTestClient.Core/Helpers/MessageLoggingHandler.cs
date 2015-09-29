using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public class MessageLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
            var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);

            byte[] requestMessage = null;

            if (request.Content != null)
                requestMessage = await request.Content.ReadAsByteArrayAsync();

            await IncommingMessageAsync(requestId, requestInfo, requestMessage);

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage;

            if (response.IsSuccessStatusCode)
                responseMessage = await response.Content.ReadAsByteArrayAsync();
            else
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

            await OutgoingMessageAsync(requestId, requestInfo, responseMessage);

            return response;
        }

        private async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            using (var output = File.AppendText(("api.log").AsAppPath()))
            {
                if (message == null)
                {
                    await output.WriteLineAsync(string.Format("{0} - Request: {1}", correlationId, requestInfo));
                }
                else
                {
                    await
                        output.WriteLineAsync(string.Format("{0} - Request: {1}\r\n{2}", correlationId, requestInfo,
                            Encoding.UTF8.GetString(message)));
                }
            }
        }

        private async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            using (var output = File.AppendText(("api.log").AsAppPath()))
            {
                if (message == null)
                {
                    await output.WriteLineAsync(string.Format("{0} - Response: {1}", correlationId, requestInfo));
                }
                else
                {
                    await
                        output.WriteLineAsync(string.Format("{0} - Response: {1}\r\n{2}", correlationId, requestInfo,
                            Encoding.UTF8.GetString(message)));
                }
            }
        }
    }
}