// Copyright (c) Peter Dongan. All rights reserved.
// Licensed under the MIT licence. https://opensource.org/licenses/MIT
// Project: https://github.com/peterdongan/ExdTech.ImageServer

using ExdTech.ImageServer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IImageStorageService _imageStore;
        private readonly ILogger _logger;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IInfoStorageService _infoStorageService;

        public ApiController(IImageStorageService imageStore,
                             ILogger<ApiController> logger,
                             IImageProcessingService imageProcessingService,
                             IInfoStorageService infoStorageService)
        {
            _imageStore = imageStore;
            _logger = logger;
            _infoStorageService = infoStorageService;
            _imageProcessingService = imageProcessingService;
        }

        /// <summary>
        /// Get serialized image file and image info
        /// </summary>
        [HttpGet]
        [Route("/{id}")]
        public async Task<ActionResult> GetImage(string id)
        {
            if (id.Length > 41)
            {
                return BadRequest ("The Id was too long");
            }

            if(id.Length <36)
            {
                return BadRequest("The Id was too short");
            }

            Guid gId;

            try
            {
                gId = Guid.Parse(id.Substring(0, 36));
            }
            catch
            {
                return BadRequest("The Id was not a valid GUID.");
            }

            

            _logger.LogInformation("GET " + id);

            try
            {
                var image = await _imageStore.GetImage(gId);
                image.Info = await _infoStorageService.GetInfo(gId);
                return  new ObjectResult (image);
            }
            catch (FileNotFoundException)
            {
                return NotFound("No image with the specified Id was found.");
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get image info without the file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/{id}/info")]
        public async Task<ActionResult> GetImageInfo (Guid id)
        {
            _logger.LogInformation("GET " + id);
            try
            {
                var info = await _infoStorageService.GetInfo(id);
                return new OkObjectResult(info);
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get image file without info
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/{id}/file")]
        public async Task<ActionResult> GetImageFile (Guid id)
        {
            _logger.LogInformation("GET " + id);
            try
            {
                var image = await _imageStore.GetImage(id);
                return File(image.FileContent, image.DocType, image.FileName);
            }
            catch (FileNotFoundException)
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
        [HttpPost, Authorize(Policy = "access")] // If using authorization: [HttpPost, Authorize(Policy = "access")]
        [Route("/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostImage(UploadedImage image)
        {

            var imageData = image.Data;
            string contentType;

            try
            {
                _imageProcessingService.ProcessImage (ref imageData, image.WidthLimitPx, image.HeightLimitPx);
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
            await _infoStorageService.AddInfo (id, image.Info);
            _logger.LogInformation(id + " added by " + User.Identity.Name);
            return Ok(id);
            
        }


    }

}
