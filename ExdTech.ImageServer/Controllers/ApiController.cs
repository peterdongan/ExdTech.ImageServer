// Copyright (c) Peter Dongan. All rights reserved.
// Licensed under the MIT licence. https://opensource.org/licenses/MIT
// Project: https://github.com/peterdongan/ExdTech.ImageServer

using ExdTech.ImageServer.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.IO;
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

        /// <summary>
        /// Get stored image.
        /// </summary>
        /// <param name="id">Id of requested image (GUID)</param>
        /// <returns>Requested image file with name [Id].jpeg</returns>
        [HttpGet]
        [Route("/{id}")]
        public async Task<ActionResult> GetImage(Guid id)
        {
            try
            {
                var image = await _imageStore.GetImage(id);
                return File(image.FileContent, image.DocType, image.FileName);
            }
            catch (FileNotFoundException e)
            {
                return NotFound("No image with the specified Id was found.");
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Upload an image
        /// </summary>
        /// <param name="image">Image serialized in a byte array with optional arguments for processing.</param>
        /// <returns>Id of the image (GUID)</returns>
        [HttpPost]//, Authorize(Policy = "access")] // If using authorization: [HttpPost, Authorize(Policy = "access")]
        [Route("/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostImage(SerializedImage image)
        {
            var imageData = image.Data;

            string contentType;

            try
            {
                _imageProcessor.ProcessImage (ref imageData, image.WidthLimitPx, image.HeightLimitPx, image.ByteLimit);
                contentType = "image/jpeg";
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);  // Assumed this is thrown because Image dimensions outside expected range.
            }
            catch (InvalidDataException)
            {
                return BadRequest("Not a valid image file.");
            }

            var id = await _imageStore.AddImage (imageData, contentType);
            var response = new { Id = id };
            return Ok(response);
            
        }


    }

}
