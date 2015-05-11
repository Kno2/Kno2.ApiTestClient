using System.ComponentModel;

namespace Kno2.ApiTestClient.Helpers
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