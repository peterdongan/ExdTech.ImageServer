using ImageMagick;


namespace ExdTech.ImageProcessing.Standard
{
    /// <summary>
    /// Processes an image
    /// </summary>
    public class MagickIan
    {
        public static byte[] ProcessImage (byte[] sImage, int maxWidth, int maxHeight, int quality)
        {
            using (var image = new MagickImage(sImage))
            {
                var size = new MagickGeometry(maxWidth, maxHeight);
                size.Less = false;
                size.Greater = true;
                image.Quality = quality;
                image.Format = MagickFormat.Jpeg;
                image.Resize(size);

                return image.ToByteArray();
            }
        }

    }
}
