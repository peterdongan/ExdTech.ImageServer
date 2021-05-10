using System;
using System.Collections.Generic;
using System.Text;

namespace ExdTech.ImageServer.Contract
{
    public enum ImageFileValidationResult
    {
        INVALID,
        jpg,
        png,
        bmp,
        gif,
        TOOBIG
    }
}
