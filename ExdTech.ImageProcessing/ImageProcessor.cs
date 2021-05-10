using ExdTech.ImageServer.Contract;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;


namespace ExdTech.ImageProcessing.Standard
{
    public class ImageProcessor : IImageProcessor
    {
        private readonly int _maxFileSizeNotCompressedInBytes;
        private readonly double _maxHeightInPixels;
        private readonly double _maxWidthInPixels;
        private readonly int _compressionQualityPercentage;

        public ImageProcessor (int maxFileSizeNotCompressedInBytes,
                             double maxWidthInPixels,
                             double maxHeightInPixels,
                             int compressionQualityPercentage)
        {
            _maxFileSizeNotCompressedInBytes = maxFileSizeNotCompressedInBytes;
            _maxHeightInPixels = maxHeightInPixels;
            _maxWidthInPixels = maxWidthInPixels;
            _compressionQualityPercentage = compressionQualityPercentage;
        }

        /// <summary>
        /// Check the image dimensions and filesize are within stated limits. If not then apply scaling and compress it to be an 80% quality jpg. Reencode to jpg regardless.
        /// </summary>
        /// <param name="serializedImage"></param>
        /// <returns>True if processing applied. False if image was not changed.</returns>
        public bool ProcessImageForSaving (ref byte[] serializedImage)
        {
            //This code won't work in UWP.

            var fileByteCount = serializedImage.Length;
            var stream = new MemoryStream(serializedImage);
            bool isChanged = false;

            Image img = Image.FromStream(stream);
            var width = img.Width;
            var height = img.Height;
            double requiredHeight;
            double requiredWidth;
            bool tooBig = false;

            if (width > _maxWidthInPixels || height > _maxHeightInPixels)
            {
                var verticalScaleFactor = _maxHeightInPixels / (double)height;
                var horizontalScaleFactor = _maxWidthInPixels / (double)width;
                var scaleFactor = Math.Min(verticalScaleFactor, horizontalScaleFactor);
                requiredHeight = height * scaleFactor;
                requiredWidth = width * scaleFactor;
                tooBig = true;
                isChanged = true;
            }
            else
            {
                requiredHeight = height;
                requiredWidth = width;
            }

            int newQuality;

            if (tooBig || fileByteCount > _maxFileSizeNotCompressedInBytes)
            {
                newQuality = _compressionQualityPercentage;
                isChanged = true;
            }
            else
            {
                newQuality = 100;
            }

            //Reencode all uploaded images as a security measure.
            var image = ReencodeImage(img, (int)requiredWidth, (int)requiredHeight, newQuality);
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                serializedImage = ms.ToArray();
                return isChanged;
            }
        }

        public static Image ReencodeImage(Image image, int newWidth, int newHeight,    //https://stackoverflow.com/questions/24643408/how-to-do-on-the-fly-image-compression-in-c
                            int newQuality)   // set quality to 1-100, eg 50
        {
            using (Image memImage = new Bitmap(image, newWidth, newHeight))
            {
                ImageCodecInfo myImageCodecInfo;
                System.Drawing.Imaging.Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                myImageCodecInfo = GetEncoderInfo("image/jpeg");
                myEncoder = System.Drawing.Imaging.Encoder.Quality;
                myEncoderParameters = new EncoderParameters(1);
                myEncoderParameter = new EncoderParameter(myEncoder, newQuality);
                myEncoderParameters.Param[0] = myEncoderParameter;

                MemoryStream memStream = new MemoryStream();
                memImage.Save(memStream, myImageCodecInfo, myEncoderParameters);
                Image newImage = Image.FromStream(memStream);
                ImageAttributes imageAttributes = new ImageAttributes();
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(newImage, new Rectangle(Point.Empty, newImage.Size), 0, 0,
                      newImage.Width, newImage.Height, GraphicsUnit.Pixel, imageAttributes);
                }
                return newImage;
            }
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in encoders)
                if (ici.MimeType == mimeType) return ici;

            return null;
        }
    }
}
