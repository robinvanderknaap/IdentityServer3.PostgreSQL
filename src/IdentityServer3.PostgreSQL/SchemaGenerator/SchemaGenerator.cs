using Npgsql;

namespace IdentityServer3.PostgreSQL.SchemaGenerator
{
    public class SchemaGenerator
    {
        private readonly string _connectionString;

        public SchemaGenerator(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void GenerateSchema(bool recreateIfExists)
        {
            var databaseExists = DatabaseExists(_connectionString);

            if (databaseExists && !recreateIfExists)
            {
                return;
            }

            if (databaseExists)
            {
                DropDatabase(_connectionString);
            }

            CreateDatabase(_connectionString);

            var builder = new NpgsqlConnectionStringBuilder(_connectionString);

            // Create connection to database server
            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();

                // Create schema
                var createCommand = connection.CreateCommand();
                createCommand.CommandText = @"
                    -- Tokens
                    CREATE TABLE tokens
                    (
                      key character varying(128) NOT NULL,
                      token_type smallint NOT NULL,
                      subjectid character varying(200) NOT NULL,
                      clientid character varying(200) NOT NULL,
                      expires timestamp with time zone NOT NULL,
                      token text NOT NULL,
                      CONSTRAINT pk_tokens_key_tokentype PRIMARY KEY (token_type, key)
                    )
                    WITH (
                      OIDS=FALSE
                    );

                    CREATE INDEX ix_tokens_subjectid_clientid_tokentype
                      ON tokens
                      USING btree
                      (subjectid COLLATE pg_catalog.""default"", clientid COLLATE pg_catalog.""default"", token_type);
                ";
                
                createCommand.ExecuteNonQuery();

                connection.Close();
            }


        }

        private bool DatabaseExists(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            builder.Database = "postgres";

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();

                var existsCommand = connection.CreateCommand();
                existsCommand.CommandText = $"SELECT 1 from pg_database WHERE datname='{databaseName}'";

                var exists = existsCommand.ExecuteScalar() != null;

                connection.Close();

                return exists;
            }
        }

        private void CreateDatabase(string connectionString)
        {
            DropDatabase(connectionString);

            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            builder.Database = "postgres";

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();

                var createCommand = connection.CreateCommand();
                createCommand.CommandText = $@"CREATE DATABASE ""{databaseName}"" ENCODING = 'UTF8'";
                createCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        private static void DropDatabase(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            builder.Database = "postgres";

            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                connection.Open();

                var killConnectionsCommand = connection.CreateCommand();
                killConnectionsCommand.CommandText =
                    $@"
                    SELECT pg_terminate_backend(pg_stat_activity.pid)
                    FROM pg_stat_activity
                    WHERE pg_stat_activity.datname = '{databaseName}' AND pid <> pg_backend_pid();";
                killConnectionsCommand.ExecuteNonQuery();

                var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = $@"drop database if exists ""{databaseName}""";
                dropCommand.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
