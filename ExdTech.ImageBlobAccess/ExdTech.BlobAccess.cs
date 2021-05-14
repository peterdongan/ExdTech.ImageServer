using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ExdTech.ImageServer.Contract;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ExdTech.ImageBs.BlobAccess
{
    public class BlobAccess : IImageStore
    {
        private readonly string _connectionString;
        private readonly string _containerClient;

        public BlobAccess(string connectionString, string containerClient)
        {
            if (connectionString == null || containerClient == null)
            {
                throw new ArgumentNullException("Connection string or container name was not specified.");
            }
            _connectionString = connectionString;
            _containerClient = containerClient;
        }

        public async Task<RetrievedImage> GetImage(Guid id)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerClient);
                BlobClient blobClient = containerClient.GetBlobClient(id.ToString());
                var download = await blobClient.DownloadAsync();
                var contentType = download.Value.ContentType;
                return new RetrievedImage { DocType = contentType, FileContent = download.Value.Content, Id = id };
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 404)
                {
                    throw new FileNotFoundException();
                }
                else throw e;
            }
        }

        public async Task<Guid> AddImage(byte[] serializedFile, string docType)
        {
            var id = Guid.NewGuid();
            var idString = id.ToString();

            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            var containerClient = blobServiceClient.GetBlobContainerClient(_containerClient);

            BlobClient blobClient = containerClient.GetBlobClient(idString);

            // Open the file and upload its data
            var stream = new MemoryStream(serializedFile);
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = docType });
            return id;
        }
    }
}
