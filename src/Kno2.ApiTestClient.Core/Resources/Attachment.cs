using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Kno2.ApiTestClient.Core.Resources
{
    /// <summary>
    /// This is a simple example of creating a serialization class to help reduce
    /// some of the friction in dealing with complex objects if you won't want
    /// work with them directly in the wire format.
    /// This particular example deals with the message body properties and the 
    /// attachment collection
    /// </summary>
    public class MessageResource
    {
        public MessageResource()
        {
            if (Patient == null)
                Patient = new Patient();
        }

        public List<AttachmentResource> Attachments { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string PatientName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public Patient Patient { get; set; }
        public string Comments { get; set; }
        public string ReasonForDisclosure { get; set; }
        public string Origin { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsUrgent { get; set; }
        public MessageStatus Status { get; set; }
        public ProcessedType ProcessedType { get; set; }
        public string[] ProcessTypes { get; set; }
        public Priority Priority { get; set; }
        public DateTime? UnprocessedNotificationSent { get; set; }
        public bool Attachments2Pdf { get; set; }
        public bool Attachments2Cda { get; set; }
        public bool IsNew { get; set; }
        public string MessageType { get; set; }
        public string Id { get; set; }
    }

    public class Patient
    {
        public string PatientId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Gender { get; set; }
        public string VisitId { get; set; }
        public string Issuer { get; set; }
        public string BirthDate { get; set; }
        public string VisitDate { get; set; }
        public string Zip { get; set; }
    }


    /// <summary>
    /// This is a simple example of creating a serialization class to help reduce
    /// some of the friction in dealing with complex objects if you won't want
    /// work with them directly in the wire format.
    /// This particular example deals with both native and pdf attachments but
    /// there are different ways of structuring this data.
    /// </summary>
    public class AttachmentResource
    {
        public AttachmentResource()
        {
            NativeFileBytes = new byte[0];
            PdfFileBytes = new byte[0];
        }

        public string Id { get; set; }
        public string Key { get; set; }
        public string DocumentType { get; set; }
        public string MimeType { get; set; }
        public bool IsClone { get; set; }
        public string PreviewKey { get; set; }
        public string[] Recipients { get; set; }
        public AttachmentMetaResource AttachmentMeta { get; set; }
        public bool IsPreviewAvailable { get; set; }
        public int SenderOrganizationId { get; set; }
        public string PreviewAvailable { get; set; }

        [JsonProperty(PropertyName = "fileName")]
        public string NativeFileName { get; set; }

        public long SizeInBytes
        {
            get
            {
                if (NativeFileBytes == null) return 0;
                return NativeFileBytes.Length;
            }
        }

        [JsonIgnore]
        public byte[] NativeFileBytes { get; set; }

        [JsonIgnore]
        public string PdfFileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NativeFileName)) return string.Empty;
                return Path.GetFileNameWithoutExtension(NativeFileName) + "_converted.pdf";
            }
        }

        [JsonIgnore]
        public byte[] PdfFileBytes { get; set; }
    }

    public class AttachmentMetaResource
    {
        public string DocumentTitle { get; set; }
        public string DocumentType { get; set; }
        public DateTime DocumentDate { get; set; }
        public string DocumentDescription { get; set; }
        public Confidentiality Confidentiality { get; set; }
        public Patient Patient { get; set; }
    }


    /// <summary>
    /// This enum is an example of data values that are known via documenation only
    ///   and client code could leverage this type of lookup to prevent invalid values
    ///   from being sent to the API.  This is not a required code artfact to use the API.
    /// </summary>
    public enum Confidentiality
    {
        /// <summary>
        /// Normal must be 0
        /// </summary>
        Normal,

        /// <summary>
        /// Normal must be 1
        /// </summary>
        Restricted,

        /// <summary>
        /// Normal must be 2
        /// </summary>
        VeryRestricted
    }

    public enum MessageStatus
    {
        None,
        Received,
        Pending,
        Signed,
        Suspended,
        Deleted,
        Removed,
        Forwarded,
        Replied,
        Processed
    }

    public enum ProcessedType
    {
        None,
        StructuredExport,
        PDFExport,
        NativeExport,
        Printed,
        Saved,
        AwaitingEMRExport,
        EMRExported
    }

    public enum Priority
    {
        NotUrgent,
        Urgent
    }


    public class DocumentTypesResource
    {
        public DocumentTypeResource[] DocumentTypes { get; set; }
        public int DocumentTypesCount { get; set; }
    }

    public class DocumentTypeResource
    {
        public string Name { get; set; }
        public string OrganizationId { get; set; }
        public string Id { get; set; }
    }
}
