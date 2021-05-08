using ExdTech.ImageBs.BlobAccess;
using ExdTech.ImageFileValidation;
using ExdTech.ImageProcessing.Standard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExdTech.ImageServer.Controllers
{
    [Route("[controller]/[action]")]
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
        public async Task<Guid> PageImages(Byte[] image)
        {
            Debug.WriteLine("POST to api/images");
            Debug.WriteLine("Authenticated as " + User.Identity.Name);

            return await ProcessPosts(image, 1080, 1080, 200000);
        }


        [HttpPost, Authorize(Policy = "access")]
        public async Task<Guid> ContentImages(Byte[] image)
        {
            Debug.WriteLine("POST to api/images");
            Debug.WriteLine("Authenticated as " + User.Identity.Name);
            // var dto = JsonConvert.DeserializeObject<FullPageDto>(dtostring, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }); 

            return await ProcessPosts(image, 720, 720, 50000);
        }

        private async Task<Guid> ProcessPosts(byte[] image, double maxWidth, double maxHeight, int maxFileSize)
        {
            ImageFileValidationResult validationResult = ImageFileValidator.CheckImage(image);

            if (validationResult == ImageFileValidationResult.INVALID)
            {
                throw new BadHttpRequestException("File validation failed. Verify that it is a valid jpg, png, gif or bmp");
            }
            if (validationResult ==  ImageFileValidationResult.TOOBIG)
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
