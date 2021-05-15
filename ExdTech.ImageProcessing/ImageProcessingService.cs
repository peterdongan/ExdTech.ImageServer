using ExdTech.ImageServer.Contract;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;


namespace ExdTech.ImageProcessing.Standard
{
    /// <summary>
    /// Determines what processing to apply and invokes a processor.
    /// </summary>
    public class ImageProcessingService : IImageProcessingService
    {
      //  private readonly int _maxFileSizeNotCompressedInBytes;
        private readonly double _maxHeightInPixels;
        private readonly double _maxWidthInPixels;
        private readonly int _compressionQualityPercentage;
        private readonly uint _maxFileSizeAcceptedInBytes;

        public ImageProcessingService(ImageProcessingOptions options)
        {
            _maxHeightInPixels = options.MaxHeightInPixels;
            _maxWidthInPixels = options.MaxWidthInPixels;
            _compressionQualityPercentage = options.CompressionQualityPercentage;
            _maxFileSizeAcceptedInBytes = options.MaxFileSizeAcceptedInBytes;
        }

        public bool ProcessImage(ref byte[] serializedImage, ushort? wLimitPx, ushort? hLimitPx)
        {
            //This code won't work in UWP.

            var fileByteCount = serializedImage.Length;
            if (fileByteCount > _maxFileSizeAcceptedInBytes)
            {
                throw new ArgumentOutOfRangeException("File size of " + fileByteCount + " is greater than expected maximum of " + _maxFileSizeAcceptedInBytes);
            }

            double applicableWidthLimit = wLimitPx == null ? _maxWidthInPixels : Math.Min(wLimitPx.Value, _maxWidthInPixels);
            double applicableHeightLimit = wLimitPx == null ? _maxHeightInPixels : Math.Min(hLimitPx.Value, _maxHeightInPixels);            

            //Reencode all uploaded images as a security measure.
            serializedImage = MagickIan.ProcessImage (serializedImage, (int)applicableWidthLimit, (int)applicableHeightLimit, _compressionQualityPercentage);
            return true;
        }
    }
}
