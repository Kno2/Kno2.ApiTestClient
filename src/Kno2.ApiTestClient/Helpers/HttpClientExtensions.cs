using System.Net.Http;
using System.Net.Http.Headers;

namespace Kno2.ApiTestClient.Helpers
{
    public static class HttpClientExtensions
    {
        public static MediaType DefaultMediaType(this HttpClient source)
        {
            if (source.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/json")))
                return MediaType.json;
            if (source.DefaultRequestHeaders.Accept.Contains(new MediaTypeWithQualityHeaderValue("application/xml")))
                return MediaType.xml;

            return MediaType.unknown;
        }
    }
}