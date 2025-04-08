using Dapper;
using Upload.S3.API.Application.Domain;

namespace Upload.S3.API.Application.Infrastructure.Repositories
{
    public class ArchiveRepository : IArchiveRepository
    {
        private readonly DbSession _dbSession;

        public ArchiveRepository(DbSession dbSession)
        {
            _dbSession = dbSession;
        }

        public async Task<Archive> AddArchive(Archive archive)
        {
            using var connection = _dbSession.Connection;
            string query = "Insert Into archives(title, path) values(@title, @path);";
            var addArchive = await connection.ExecuteAsync(sql: query, param: archive);

            if (addArchive == 0)
                throw new Exception("Fail in add file");

            return archive;
        }

        public async Task<IEnumerable<Archive>> GetAll()
        {
            using var connection = _dbSession.Connection;
            string query = "select title, path from archives;";

            var archives = await connection.QueryAsync<Archive>(sql: query);

            return archives;
        }
    }
}
