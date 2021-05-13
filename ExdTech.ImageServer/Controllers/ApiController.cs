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

        [HttpPost]
        [Route("/{id}")]    // If using authorization: [HttpPost, Authorize(Policy = "access")]
        public async Task<Guid> PostImage(SerializedImage image)
        {
            var imageData = image.Data;

            string contentType;

            try
            {
                _imageProcessor.ProcessImageForSaving(ref imageData, image.WidthLimitPx, image.HeightLimitPx);
                contentType = "image/jpeg";
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new BadHttpRequestException (e.Message);   // Assumed this is thrown because Image dimensions outside expected range.
            }
            catch (ArgumentException)
            {
                throw new BadHttpRequestException ("Not a valid image file.");
            }
            catch (OutOfMemoryException)
            {
                throw new BadHttpRequestException ("Not a valid image file.");
            }


            var id = await _imageStore.AddImage (imageData, contentType);

            return id;
        }


    }

}
