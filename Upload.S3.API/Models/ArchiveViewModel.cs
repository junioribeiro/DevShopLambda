using System.Reflection.Metadata.Ecma335;

namespace Upload.S3.API.Models
{
    public class ArchiveViewModel
    {
        public string Title { get; set; }
        public IFormFile Path { get; set; }

    }
}
