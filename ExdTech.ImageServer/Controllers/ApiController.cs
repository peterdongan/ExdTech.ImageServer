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
        private readonly IUploadValidator _imageFileValidator;


        public ApiController(IImageStore imageStore,
                             IImageProcessor imageProcessor,
                             IUploadValidator uploadValidator)
        {
            _imageStore = imageStore;
            _imageProcessor = imageProcessor;
            _imageFileValidator = uploadValidator;
        }

        [HttpGet]
        [Route("/{id}")]
        public async Task<FileResult> GetImage(Guid id)
        {
            var image = await _imageStore.GetImage(id);
            return File(image.FileContent, image.DocType, image.FileName);
        }

        [HttpPost]
        [Route("/{id}")]    // If using authorization: [HttpPost, Authorize(Policy = "access")]
        public async Task<Guid> PostImage(SerializedImage image)
        {
            var imageData = image.Data;

            ImageFileValidationResult validationResult = _imageFileValidator.CheckImage(imageData);

            if (validationResult == ImageFileValidationResult.INVALID)
            {
                throw new BadHttpRequestException("File validation failed. Verify that it is a valid jpg, png, gif or bmp");
            }
            if (validationResult == ImageFileValidationResult.TOOBIG)
            {
                throw new BadHttpRequestException("File too large. Max accepted filesize is 5MB.");
            }

            string contentType;

            try
            {
                if (_imageProcessor.ProcessImageForSaving(ref imageData))
                {
                    contentType = "image/jpeg";
                }
                else
                {
                    contentType = ImageFileValidator.GetContentTypeFromValidationResult(validationResult);
                }
            }
            catch (ArgumentException)
            {
                throw new BadHttpRequestException("Not a valid image file.");
            }
            catch (OutOfMemoryException)
            {
                throw new BadHttpRequestException("Not a valid image file.");
            }

            var id = await _imageStore.AddImage (imageData, contentType);

            return id;
        }


    }

}
