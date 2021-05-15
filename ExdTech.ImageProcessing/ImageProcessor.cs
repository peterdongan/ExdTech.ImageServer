using ExdTech.ImageServer.Contract;
using ImageMagick;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;


namespace ExdTech.ImageProcessing.Standard
{
    /// <summary>
    /// Processes an image
    /// </summary>
    public class ImageProcessor
    {
        public static byte[] MagickIan(byte[] sImage, int maxWidth, int maxHeight, int quality)
        {
            using (var image = new MagickImage(sImage))
            {
                var size = new MagickGeometry(maxWidth, maxHeight);
                size.Less = false;
                image.Quality = quality;
                image.Resize(size);

                return image.ToByteArray();
            }
        }

        public static Image ReencodeImage(Image image, int newWidth, int newHeight,    //https://stackoverflow.com/questions/24643408/how-to-do-on-the-fly-image-compression-in-c
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
