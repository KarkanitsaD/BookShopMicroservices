using System.IO;

namespace BookService.Business
{
    public class BlobInfo
    {
        public Stream Content { get; set; }
        public string ContentType { get; set; }
    }
}