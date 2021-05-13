using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Contract
{
    public interface IImageProcessor
    {
        /// <summary>
        /// If this is set then an exception is thrown if an image's width exceeds it.
        /// </summary>
        public ushort? MaxWidthExpected { get; }

        /// <summary>
        /// If this is set then an exception is thrown if an image's width exceeds it.
        /// </summary>
        public ushort? MaxHeightExpected { get; }

        /// <summary>
        /// Throws ArgumentOutOfRangeException if MaxWidthExpected/MaxHeightExpected are set and the dimensions of the serialized image exceed them.
        /// </summary>
        /// <param name="serializedImage"></param>
        /// <param name="widthlimit">If this is less than the image width, then the image is scaled down</param>
        /// <param name="heightLimit">If this is less than the image height, then the image is scaled down</param>
        /// <param name="byteLimit">If this limit is exceeded, the image is compressed. </param>
        /// <returns></returns>
        public bool ProcessImageForSaving (ref byte[] serializedImage, ushort? widthlimit, ushort? heightLimit, uint? byteLimit);
    }
}
