using System.Reflection.Metadata.Ecma335;

namespace Upload.S3.API.Application.Domain
{
    public class Archive
    {
        public string Title { get; set; }
        public string Path { get; set; }

        public Archive(string title, string path)
        {
            Title = title;
            Path = path;
        }
    }
}
