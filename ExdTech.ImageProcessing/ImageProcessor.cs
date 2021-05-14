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
        private readonly ushort? _maxWidthAccepted;
        private readonly ushort? _maxHeightAccepted;
        private readonly uint _maxFileSizeAcceptedInBytes;

        public ImageProcessor (ImageProcessingOptions options)
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
        public bool ProcessImage (ref byte[] serializedImage, ushort? wLimitPx, ushort? hLimitPx, uint? byteLimit)
        {
            //This code won't work in UWP.

            var fileByteCount = serializedImage.Length;
            var stream = new MemoryStream(serializedImage);
            if(stream.Length > _maxFileSizeAcceptedInBytes)
            {
                throw new ArgumentOutOfRangeException("File size is greater than expected maximum.");
            }

            bool isChanged = false;

            Image img = null;

            try
            {
                img = Image.FromStream(stream);   // Throws ArgumentException or OutOfMemoryException if the stream doesn't have a valid image format.

            }
            catch (ArgumentException)
            {
                throw new InvalidDataException();
            }
            catch (OutOfMemoryException)
            {
                throw new InvalidDataException();
            }

            var width = img.Width;
            var height = img.Height;

            // If _maxWidthAccepted is set then the client should not send images larger than that.
            if (_maxWidthAccepted != null && width > _maxWidthAccepted)
            {
                throw new ArgumentOutOfRangeException ("Image dimensions outside expected range.");
            }
            if (_maxHeightAccepted != null && height > _maxHeightAccepted)
            {
                throw new ArgumentOutOfRangeException ("Image dimensions outside expected range.");
            }

            double requiredHeight;
            double requiredWidth;
            bool tooBig = false;

            double applicableWidthLimit = wLimitPx == null ? _maxWidthInPixels : Math.Min(wLimitPx.Value, _maxWidthInPixels);
            double applicableHeightLimit = wLimitPx == null ? _maxHeightInPixels : Math.Min(hLimitPx.Value, _maxHeightInPixels);

            if (width > _maxWidthInPixels || height > _maxHeightInPixels)
            {
                var verticalScaleFactor = applicableHeightLimit / (double)height;
                var horizontalScaleFactor = applicableWidthLimit / (double)width;
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
            var image = ReencodeImage (img, (int)requiredWidth, (int)requiredHeight, newQuality);
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                serializedImage = ms.ToArray();
                return isChanged;
            }
        }

        public static Image ReencodeImage (Image image, int newWidth, int newHeight,    //https://stackoverflow.com/questions/24643408/how-to-do-on-the-fly-image-compression-in-c
                            int newQuality)   // set quality to 1-100, eg 50
        {
            using (Image memImage = new Bitmap(image, newWidth, newHeight))
            {
                ImageCodecInfo myImageCodecInfo;
                Encoder myEncoder;
                EncoderParameter myEncoderParameter;
                EncoderParameters myEncoderParameters;
                myImageCodecInfo = GetEncoderInfo("image/jpeg");
                myEncoder = Encoder.Quality;
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
