using System.Threading.Tasks;
using BookService.Business;
using Microsoft.AspNetCore.Mvc;

namespace BookService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlobsController : ControllerBase
    {
        private readonly IBlobService _blobService;

        public BlobsController(IBlobService blobService)
        {
            _blobService = blobService;
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<FileStreamResult> GetBlob([FromRoute] string name)
        {
            var blob = await _blobService.GetBlobAsync(name);
            return File(blob.Content, blob.ContentType);
        }

        [HttpPost]
        [Route("{fileName}")]
        public async Task UploadBlob([FromBody] string data, [FromRoute] string fileName)
        {
            await _blobService.UploadBlobContentAsync(data, fileName);
        }
    }
}