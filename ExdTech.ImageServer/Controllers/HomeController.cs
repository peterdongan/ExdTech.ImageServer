using ExdTech.ImageBs.BlobAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ExdTech.ImageServer
{
    public class HomeController : Controller
    {
        private readonly BlobAccess _imageBlobAccess;

        public HomeController (IConfiguration configuration)
        {

            var blobConnectionString = configuration["BlobConnectionString"];
            _imageBlobAccess = new BlobAccess(blobConnectionString);
        }

            [HttpGet]
        public async Task<ActionResult> ViewImage(Guid id)
        {
            var image = await _imageBlobAccess.GetBlob(id);
            var contentType = image.ContentType;
            var slashIndex = contentType.IndexOf('/');
            var fileExtension = contentType.Substring(slashIndex + 1);
            return File(image.Content, contentType, string.Format("{0}.{1}", id, fileExtension));
        }


    }
}
