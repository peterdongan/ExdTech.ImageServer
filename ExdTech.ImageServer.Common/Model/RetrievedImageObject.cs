using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExdTech.ImageServer.Common
{
    /// <summary>
    /// Image as byte array and info
    /// </summary>
    public class RetrievedImageObject
    {
        public byte[] FileContentByteArray { get; set; }

        public Guid Id { get; set; }

        public string DocType { get; set; }

        public string FileName {
            get
            {
                var slashIndex = DocType.IndexOf("/");
                var fileExtension = DocType.Substring(slashIndex + 1);
                return string.Format("{0}.{1}", Id, fileExtension);
            } }

        public RetrievedInfo Info {get; set;}


        public void InitializeFile (RetrievedImageFile f)
        {
            DocType = f.DocType;
            Id = f.Id;
            using (var memStream = new MemoryStream())
            {
                f.FileContentStream.CopyTo(memStream);
                FileContentByteArray = memStream.ToArray();
            }
        }
    }
}
