using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExdTech.ImageServer.Contract
{
    public class RetrievedImage
    {
        public Stream FileContent { get; set; }

        public Guid Id { get; set; }

        public string DocType { get; set; }

        public string FileName { 
            get
            {
                var slashIndex = DocType.IndexOf("/");
                var fileExtension = DocType.Substring(slashIndex + 1);
                return  string.Format("{0}.{1}", Id, fileExtension);
            } }


    }
}
