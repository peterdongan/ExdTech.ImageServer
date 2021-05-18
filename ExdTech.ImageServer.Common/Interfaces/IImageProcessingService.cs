using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Common
{
    public interface IImageProcessingService
    {

        /// <summary>
        /// Throw ArgumentOutOfRangeException if MaxAcceptedWidth/MaxAcceptedHeight are set and the dimensions of the serialized image exceed them.
        /// Throw InvalidDataException if it is not an image file 
        /// </summary>
        /// <param name="serializedImage"></param>
        /// <param name="widthlimit">If this is less than the image width, then the image is scaled down</param>
        /// <param name="heightLimit">If this is less than the image height, then the image is scaled down</param>
        /// <param name="byteLimit">If this limit is exceeded, the image is compressed. </param>
        /// <returns></returns>
        public bool ProcessImage (ref byte[] serializedImage, ushort? widthlimit, ushort? heightLimit);
    }
}
