using ExdTech.ImageBs.BlobAccess;
using ExdTech.ImageFileValidation;
using ExdTech.ImageProcessing.Standard;
using ExdTech.ImageServer.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly BlobAccess _blobAccess;

        public ApiController(IConfiguration configuration)
        {
            Debug.WriteLine("Images api controller constructed.");
            var blobConnectionString = configuration["BlobConnectionString"];
            _blobAccess = new BlobAccess(blobConnectionString);
        }

        [HttpPost, Authorize(Policy = "access")]
        public async Task<Guid> PageImages(SerializedImage image)
        {
            Debug.WriteLine("POST to api/images");
            Debug.WriteLine("Authenticated as " + User.Identity.Name);

            return await ProcessPosts(image.Data, 1080, 1080, 200000);
        }


        [HttpGet]
        [Route("/[action]/{id}")]
        public async Task<FileResult> ContentImages(Guid id)
        {
            return await GetImage(id);
        }

        [HttpGet]
        [Route("/[action]/{id}")]
        public async Task<FileResult> PageImages(Guid id)
        {
            return await GetImage(id);
        }

        [HttpGet]
        [Route("/{id}")]
        public async Task<FileResult> GetImage(Guid id)
        {
            var image = await _blobAccess.GetBlob(id);
            var contentType = image.ContentType;
            var slashIndex = contentType.IndexOf('/');
            var fileExtension = contentType.Substring(slashIndex + 1);
            return File(image.Content, contentType, string.Format("{0}.{1}", id, fileExtension));
        }

        /// <summary>
        /// This is the same as posting to /contentimages
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost, Authorize(Policy = "access")]
        [Route("/{id}")]
        public async Task<Guid> PostImage(SerializedImage image)
        {
            return await ProcessPosts(image.Data, 720, 720, 50000);
        }


        [HttpPost, Authorize(Policy = "access")]
        public async Task<Guid> ContentImages(SerializedImage image)
        {
            return await ProcessPosts(image.Data, 720, 720, 50000);
        }

        private async Task<Guid> ProcessPosts(byte[] image, double maxWidth, double maxHeight, int maxFileSize)
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

            //Shrink/compress the image if necessary. If it is shrunk/compressed then it is converted to a jpg.
            if (ImageProcessor.ProcessImageForSaving(ref image, maxWidth, maxHeight, maxFileSize))
            {
                contentType = "image/jpeg";
            }
            else
            {
                contentType = ImageFileValidator.GetContentTypeFromValidationResult(validationResult);
            }

            var id = await _blobAccess.AddBlob(image, contentType);

            return id;
        }

    }

}
