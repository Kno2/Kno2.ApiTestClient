using System;
using System.ComponentModel;

namespace Kno2.ApiTestClient.Core
{
    /// <summary>
    /// Known file extensions in the samples source folder
    /// </summary>
    public enum FileType
    {
        [Description("MS Word")]
        docx,
        [Description("Html File")]
        html,
        [Description("Jpg Image")]
        jpg,
        [Description("Adobe PDF")]
        pdf,
        [Description("Plain Text File")]
        txt,
        [Description("Xml File")]
        xml,
        [Description("MS Excel")]
        xlsx
    }
}