using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Kno2.ApiTestClient.Core.Helpers
{
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