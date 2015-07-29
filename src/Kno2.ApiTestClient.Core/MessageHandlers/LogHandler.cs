using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Kno2.ApiTestClient.Core.Helpers;

namespace Kno2.ApiTestClient.Core.MessageHandlers
{
    public class LogHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Run(() => request.WriteApiEntry(), cancellationToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}