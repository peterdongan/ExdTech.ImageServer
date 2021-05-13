using System;
using System.Collections.Generic;
using System.Text;

namespace ExdTech.ImageServer.Contract
{
    public enum ImageFileValidationResult
    {
        Invalid,
        jpg,
        png,
        bmp,
        gif,
        FileSizeTooBig,
        WidthTooBig,
        HeightTooBig
    }
}
