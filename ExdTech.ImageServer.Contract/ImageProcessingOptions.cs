using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Contract
{
    public class ImageProcessingOptions
    {

        public const string ImageProcessingConfig = "ImageProcessingConfig";
        public double MaxHeightInPixels { get; set; }
        public double MaxWidthInPixels { get; set; }
        public int CompressionQualityPercentage { get; set; }
        public uint MaxFileSizeAcceptedInBytes { get; set; }
    }
}
