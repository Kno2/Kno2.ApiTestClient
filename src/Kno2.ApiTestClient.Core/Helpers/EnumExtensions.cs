using System;
using System.ComponentModel;

namespace Kno2.ApiTestClient.Core.Helpers
{
    public static class EnumExtensions
    {
        public static string Description(this Enum value)
        {
            var descriptionAttributes = (DescriptionAttribute[])
                (value.GetType().GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false));
            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : value.ToString();
        }
    }
}