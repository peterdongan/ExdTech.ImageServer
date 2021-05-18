using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExdTech.ImageServer.Common
{
    public class Info
    {
        public string AddedBy { get; set; }
        public string Author { get; set; }
        public DateTime DateAdded { get; set; }
        public string OriginalFileName { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }

        public LicenceType LicenceId { get; set; }
    }
}
