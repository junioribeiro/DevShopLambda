namespace Upload.S3.API.Application.Domain
{
    public interface IAmazonS3Service
    {
        Task<bool> UploadFileAsync(string bucket, string key, IFormFile file);
    }
}
