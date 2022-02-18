using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


namespace BookService.Business
{
    public class BlobService : IBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        public async Task<BlobDownloadInfo> GetBlobAsync(string name)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("bookshopmicroservices");
            var blobClient = containerClient.GetBlobClient(name);
            var downloadBlobInfo = await blobClient.DownloadAsync();
            return downloadBlobInfo;
        }

        public Task<IEnumerable<string>> GetBlobNamesAsync()
        {
            throw new System.NotImplementedException();
        }


        public async Task UploadBlobContentAsync(string content, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("bookshopmicroservices");
            await containerClient.UploadBlobAsync(fileName, new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }
    }
}