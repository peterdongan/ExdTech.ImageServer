using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Contract
{
    public interface IImageProcessor
    {
        public bool ProcessImageForSaving (ref byte[] serializedImage);
    }
}
