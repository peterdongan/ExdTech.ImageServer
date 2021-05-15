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
        private readonly int _maxFileSizeNotCompressedInBytes;
        private readonly double _maxHeightInPixels;
        private readonly double _maxWidthInPixels;
        private readonly int _compressionQualityPercentage;
        private readonly ushort? _maxWidthAccepted;
        private readonly ushort? _maxHeightAccepted;
        private readonly uint _maxFileSizeAcceptedInBytes;

        public ImageProcessingService(ImageProcessingOptions options)
        {
            _maxFileSizeNotCompressedInBytes = options.MaxFileSizeNotCompressedInBytes;
            _maxHeightInPixels = options.MaxHeightInPixels;
            _maxWidthInPixels = options.MaxWidthInPixels;
            _compressionQualityPercentage = options.CompressionQualityPercentage;
            _maxWidthAccepted = options.MaxWidthAccepted;
            _maxHeightAccepted = options.MaxHeightAccepted;
            _maxFileSizeAcceptedInBytes = options.MaxFileSizeAcceptedInBytes;
        }

        public ushort? MaxAcceptedWidth => _maxWidthAccepted;
        public ushort? MaxAcceptedHeight => _maxHeightAccepted;

        /// <summary>
        /// Check the image dimensions and filesize are within the lesser of the passed limits and the  If not then apply scaling and compress it to be an 80% quality jpg. Reencode to jpg regardless.
        /// </summary>
        /// <param name="serializedImage"></param>
        /// <returns>True if processing applied. False if image was not changed.</returns>
        public bool ProcessImage(ref byte[] serializedImage, ushort? wLimitPx, ushort? hLimitPx, uint? byteLimit)
        {
            //This code won't work in UWP.

            var fileByteCount = serializedImage.Length;
            var stream = new MemoryStream(serializedImage);
            if (stream.Length > _maxFileSizeAcceptedInBytes)
            {
                throw new ArgumentOutOfRangeException("File size is greater than expected maximum.");
            }

            bool tooBig = false;

            double applicableWidthLimit = wLimitPx == null ? _maxWidthInPixels : Math.Min(wLimitPx.Value, _maxWidthInPixels);
            double applicableHeightLimit = wLimitPx == null ? _maxHeightInPixels : Math.Min(hLimitPx.Value, _maxHeightInPixels);

            int newQuality;

            if (tooBig || fileByteCount > _maxFileSizeNotCompressedInBytes)
            {
                newQuality = _compressionQualityPercentage;
            }
            else
            {
                newQuality = 100;
            }

            //Reencode all uploaded images as a security measure.
            serializedImage = ImageProcessor.MagickIan(serializedImage, (int)applicableWidthLimit, (int)applicableHeightLimit, newQuality);
            return true;
        }
    }
}
