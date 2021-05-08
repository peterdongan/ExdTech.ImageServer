using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExdTech.ImageBs.BlobAccess
{
    public class BlobAccess
    {
        private string _connectionString;

        public BlobAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<BlobDownloadInfo> GetBlob(Guid id)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("imagefiles");
            BlobClient blobClient = containerClient.GetBlobClient(id.ToString());
            var download = await blobClient.DownloadAsync();
            return download.Value;

        }

        public async Task<Guid> AddBlob(byte[] imageByteArray, string docType)
        {
            var id = Guid.NewGuid();
            var idString = id.ToString();

            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            var containerClient = blobServiceClient.GetBlobContainerClient("imagefiles");

            BlobClient blobClient = containerClient.GetBlobClient(idString);

            // Open the file and upload its data
            var stream = new MemoryStream(imageByteArray);
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = docType });
            return id;
        }
    }
}
