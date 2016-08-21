using IdentityServer3.PostgreSQL.SchemaGenerator;

namespace IdentityServer3.PostgreSQL
{
    public class PostgreSqlServiceOptions
    {
        public string ConnectionString { get; set; }
        public string Schema { get; set; }
        public SchemaCreationOptions SchemaCreationOptions { get; set; }
    }
}