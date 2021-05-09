// Copyright (c) Peter Dongan. All rights reserved.
// Licensed under the MIT licence. https://opensource.org/licenses/MIT
// Project: https://github.com/peterdongan/ExdTech.ImageServer

using ExdTech.ImageFileValidation;
using ExdTech.ImageProcessing.Standard;
using ExdTech.ImageServer.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IImageStore _imageStore;
        private readonly IImageProcessor _imageProcessor;


        public ApiController(IImageStore imageStore,
                             IImageProcessor imageProcessor)
        {
            _imageStore = imageStore;
            _imageProcessor = imageProcessor;
        }

        [HttpGet]
        [Route("/{id}")]
        public async Task<FileResult> GetImage(Guid id)
        {
            var image = await _imageStore.GetImage(id);
            return File(image.FileContent, image.DocType, image.FileName);
        }

        /// <summary>
        /// This is the same as posting to /contentimages
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/{id}")]
        public async Task<Guid> PostImage(SerializedImage image)
        {
            ImageFileValidationResult validationResult = ImageFileValidator.CheckImage(image);

            if (validationResult == ImageFileValidationResult.INVALID)
            {
                throw new BadHttpRequestException("File validation failed. Verify that it is a valid jpg, png, gif or bmp");
            }
            if (validationResult == ImageFileValidationResult.TOOBIG)
            {
                throw new BadHttpRequestException("File too large. Max accepted filesize is 5MB.");
            }

            string contentType;

            //Reencode as a jpg. Shrink if larger than width/height limits. Compress if size in bytes greater than filesize limit
            if (ImageProcessor.ProcessImageForSaving(ref image, maxPixelWidth, maxPixelHeight, maxFileSizeBytes))
            {
                contentType = "image/jpeg";
            }
            else
            {
                contentType = ImageFileValidator.GetContentTypeFromValidationResult(validationResult);
            }

            var id = await _imageStore.AddImage(image, contentType);

            return id;
        }


    }

}
