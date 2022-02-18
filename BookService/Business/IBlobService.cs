using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

namespace BookService.Business
{
    public interface IBlobService
    {
        Task<BlobDownloadInfo> GetBlobAsync(string name);
        Task<IEnumerable<string>> GetBlobNamesAsync();
        Task UploadBlobContentAsync(string content, string fileName);
    }
}