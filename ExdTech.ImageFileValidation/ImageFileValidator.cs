using System;
using System.Linq;
using System.Text;

namespace ExdTech.ImageFileValidation
{
    public static class ImageFileValidator
    {
        public static string GetContentTypeFromValidationResult(ImageFileValidationResult result)
        {
            string[] contentTypes =
            {
                null,
                "image/jpeg",
                "image/png",
                "image/bmp",
                "image/gif",
                null
            };

            return contentTypes[(int)result];
        }

        public static ImageFileValidationResult CheckImage(byte[] imageByteArray)
        {
            if(imageByteArray.Length > 5000000)
            {
                return ImageFileValidationResult.TOOBIG;
            }
            if (imageByteArray.Length > 0)
            {
                byte[] header = new byte[4];

                for (var i = 0; i < 4; i++)
                {
                    header[i] = imageByteArray[i];
                }

                var headerString = Encoding.ASCII.GetString(header);

                var jpgHeaderString = "\xFF\xD8";
                var bmpHeaderString = "BM";
                var gifHeaderString = "GIF";
                var pngHeaderString = Encoding.ASCII.GetString(new byte[] { 137, 80, 78, 71 });

                if (headerString.StartsWith(jpgHeaderString))
                {
                    return ImageFileValidationResult.jpg;
                }
                if(headerString.StartsWith(bmpHeaderString))
                {
                    return ImageFileValidationResult.bmp;
                }
                if (headerString.StartsWith(gifHeaderString))
                {
                    return ImageFileValidationResult.gif;
                }
                if (headerString.StartsWith(pngHeaderString))
                {
                    return ImageFileValidationResult.png;
                }

                //jpg test above fails at least sometimes - this is another way of testing for jpg that passed my testing
                UInt16 soiForJpg = 0xd8ff;

                var soiOfFile = BitConverter.ToUInt16(header, 0);

                //Not using the following code. Some valid jpgs do not contain jfifs at this point:
                //var jfiffOfFile = BitConverter.ToUInt16(header, 2);
                //UInt16 jfifForJpg1 = 0xe0ff;
                //UInt16 jfifForJpg2 = 57855;

                if (soiForJpg == soiOfFile)
                {
                    return ImageFileValidationResult.jpg;
                }

                return ImageFileValidationResult.INVALID;
            }

            return ImageFileValidationResult.INVALID;
        }

    }
}
