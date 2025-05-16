namespace Upload.S3.API.Application.Domain
{
    public interface IAmazonS3Service
    {
        Task<bool> UploadFileAsync(string bucket, string key, IFormFile file);
        /// <summary>
        /// Gera url pre assinada do objeto
        /// </summary>
        /// <param name="bucket">bucket</param>
        /// <param name="key">chave do objeto</param>
        /// <param name="duration">Especifique quanto tempo o URL assinado será válido em horas.</param>
        /// <returns>url pre assinada para acesso publico do objeto</returns>
        Task<string> GeneratePresignedURL(string bucket, string key, double duration = 1);
    }
}
