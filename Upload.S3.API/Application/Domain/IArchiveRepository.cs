namespace Upload.S3.API.Application.Domain
{
    public interface IArchiveRepository
    {
        Task<Archive> AddArchive(Archive archive);
        Task<IEnumerable<Archive>> GetAll();
    }
}
