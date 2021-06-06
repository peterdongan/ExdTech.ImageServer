using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExdTech.ImageServer.Common
{

    public class RetrievedInfo : BaseInfo
    {
        public DateTime DateAddedUtc { get; set; }
        public string AddedBy { get; set; }
    }
}
