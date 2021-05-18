using System;

namespace ExdTech.ImageServer.Common
{
    /// <summary>
    /// Schema for POSTs to the API
    /// </summary>
    public class UploadedImage
    {
        public byte[] Data { get; set; }

        /// <summary>
        /// Scale down the image if it is wider than this. Ignored if greater than server max.
        /// </summary>
        public ushort? WidthLimitPx { get; set; }

        /// <summary>
        /// Scale down the image if it is taller than this. Ignored if greater than server max.
        public ushort? HeightLimitPx { get; set; }

        public Info Info {get; set; }
    }
}
