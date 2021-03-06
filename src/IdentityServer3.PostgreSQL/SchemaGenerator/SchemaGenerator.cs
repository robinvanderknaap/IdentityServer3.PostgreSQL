﻿using System;
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
            if (!TableExists("public", "tokens") || recreateIfExists)
            {
                CreateTokenTable();
            }
        }

        private void CreateTokenTable()
        {
            // Create connection to database server
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var dropCommand = connection.CreateCommand();
                dropCommand.CommandText = @"drop table if exists ""tokens""";
                dropCommand.ExecuteNonQuery();

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

        private bool TableExists(string schemaName, string tableName)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var existsCommand = connection.CreateCommand();
                existsCommand.CommandText = $@"
                    select exists (
                        select 1
                    from 
                        pg_catalog.pg_class c
                    join 
                        pg_catalog.pg_namespace n ON n.oid = c.relnamespace
                    where 
                        n.nspname = '{schemaName}'
                        and c.relname = '{tableName}'
                        and c.relkind = 'r'-- only tables(?)
                );";

                var exists = Convert.ToBoolean(existsCommand.ExecuteScalar());

                connection.Close();

                return exists;
            }
        }
    }
}
