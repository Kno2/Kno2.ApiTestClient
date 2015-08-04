using System;
using System.Net;
using Kno2.ApiTestClient.Core.Extensions;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public enum TokenRequestType
    {
        None,
        Password,
        ClientCredential,
        RefreshToken
    }
}