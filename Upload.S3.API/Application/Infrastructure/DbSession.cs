using Npgsql;

namespace Upload.S3.API.Application.Infrastructure
{
    public class DbSession : IDisposable
    {
        public NpgsqlConnection Connection { get; set; }
        public DbSession(IConfiguration configuration)
        {
            Connection = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
            Connection.Open();
        }

        public void Dispose()
        {
            Connection?.Dispose();
        }
    }
}
