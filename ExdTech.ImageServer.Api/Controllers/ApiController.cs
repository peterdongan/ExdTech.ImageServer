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
        public async Task<ActionResult> GetImageObject (string id)
        {
            Guid gId;

            try
            {
                gId = TryGetGuid (id);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(new ProblemDetails { Title = e.Message });
            }

            _logger.LogInformation("GET " + id);

            try
            { 
                var imageFile = await _imageStore.GetImageFile(gId);
                var imageObject = new RetrievedImageObject();
                imageObject.InitializeFile(imageFile);
                imageObject.Info = await _infoStorageService.GetInfo(gId);
                return  new JsonResult (imageObject);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new ProblemDetails { Title = "No image with the specified Id was found." });
    }
            catch (Exception )
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
        [Route("/info/{id}")]
        public async Task<ActionResult> GetImageInfo (string id)
        {
            _logger.LogInformation("GET " + id);

            Guid gId;

            try
            {
                gId = TryGetGuid(id);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(new ProblemDetails { Title = e.Message });
            }

            try
            {
                var info = await _infoStorageService.GetInfo(gId);

                if (info == null)
                {
                    return NotFound (new ProblemDetails { Title = "No info was found for " + id });
                }

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
        [Route("/images/{id}")]
        public async Task<ActionResult> GetImageFile(string id)
        {
            _logger.LogInformation("GET " + id);

            Guid gId;

            try
            {
                gId = TryGetGuid(id);
            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(new ProblemDetails { Title = e.Message });
            }

            try
            {
                var image = await _imageStore.GetImageFile(gId);
                return File(image.FileContentStream, image.DocType, image.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new ProblemDetails { Title = "No image with the specified Id was found." });
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
        /// <returns>Image object after processing and saving</returns>
        [HttpPost, Authorize(Policy = "access")] // If using authorization: [HttpPost, Authorize(Policy = "access")]
        [Route("/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PostImage(UploadedImageObject image)
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
                return BadRequest(new ProblemDetails { Title = e.Message });  // Assumed this is thrown because Image dimensions outside expected range.
            }
            catch (InvalidDataException)
            {
                return BadRequest(new ProblemDetails { Title = "Not a valid image file." });
            }

            var id = await _imageStore.AddImage (imageData, contentType);
            await _infoStorageService.AddInfo (id, image.Info, User.Identity.Name);
            image.Data = imageData;
            _logger.LogInformation(id + " added by " + User.Identity.Name);
            return Ok(image);
            
        }

        [HttpPut, Authorize(Policy = "access")]
        [Route("/{id}")]
        public async Task<ActionResult> PutImageObject(UploadedImageObject imageObject)
        {
            Guid gId;

            try
            {
                gId = TryGetGuid(RouteData.Values["id"].ToString());
                if (!ValidateInfo(imageObject.Info))
                {
                    return BadRequest(new ProblemDetails { Title = "Info contained invalid data" });
                }

            }
            catch (BadHttpRequestException e)
            {
                return BadRequest(new ProblemDetails { Title = e.Message });
            }

            var imageData = imageObject.Data;
            string contentType;

            try
            {
                _imageProcessingService.ProcessImage(ref imageData, imageObject.WidthLimitPx, imageObject.HeightLimitPx);
                contentType = "image/jpeg";
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(new ProblemDetails { Title = e.Message });  // Assumed this is thrown because Image dimensions outside expected range.
            }
            catch (InvalidDataException)
            {
                return BadRequest(new ProblemDetails { Title = "Not a valid image file." });
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            try
            {
                await _imageStore.AddImage(gId, imageData, contentType);
                await _infoStorageService.AddInfo(gId, imageObject.Info, User.Identity.Name);
            }
            catch (Exception e)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation(gId + " added by " + User.Identity.Name);
            return Ok();

        }


        private static Guid TryGetGuid (string idString)
        {
            if (idString.Length > 41)
            {
                throw new BadHttpRequestException ("The Id string was too long.");
            }

            if (idString.Length < 36)
            {
                throw new BadHttpRequestException("The Id string was too short.");
            }

            Guid gId;

            try
            {
                gId = Guid.Parse(idString.Substring(0, 36));
            }
            catch
            {
                throw new BadHttpRequestException ("The Id string was not a valid GUID.");
            }

            return gId;
        }

        private static bool ValidateInfo (UploadedInfo o)
        {
            // There is a default max upload allowed of 28MB on both IIS and Kestrel.
            // We should set a lower limit but it is not urgent because of the default server limit
            if (o == null)
            {
                return true;
            }
            if (o.Notes != null && o.Notes.Length > 1000)
            {
                return false;
            }
            if (o.Source != null && o.Source.Length > 500)
            {
                return false;
            }
            if (o.Title != null && o.Title.Length >500)
            {
                return false;
            }
            if (o.OriginalFileName != null && o.OriginalFileName.Length >200)
            {
                return false;
            }
            return true;;
        }

    }

}
