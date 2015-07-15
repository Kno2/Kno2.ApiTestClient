using System;
using System.ComponentModel;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public enum MediaType
    {
        unknown,
        [Description("application/json")]
        json,
        [Description("application/xml")]
        xml
    }
}