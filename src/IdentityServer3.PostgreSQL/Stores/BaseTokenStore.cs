using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.PostgreSQL.Models;
using IdentityServer3.PostgreSQL.Serialization;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace IdentityServer3.PostgreSQL.Stores
{
    public abstract class BaseTokenStore<T> where T : class
    {
        protected readonly PostgreSqlServiceOptions PostgreSqlServiceOptions;
        protected readonly TokenType TokenType;
        protected readonly IScopeStore ScopeStore;
        protected readonly IClientStore ClientStore;

        protected BaseTokenStore(PostgreSqlServiceOptions postgreSqlServiceOptions, TokenType tokenType, IScopeStore scopeStore, IClientStore clientStore)
        {
            if (postgreSqlServiceOptions == null) throw new ArgumentNullException(nameof(postgreSqlServiceOptions));
            if (scopeStore == null) throw new ArgumentNullException(nameof(scopeStore));
            if (clientStore == null) throw new ArgumentNullException(nameof(clientStore));

            PostgreSqlServiceOptions = postgreSqlServiceOptions;
            TokenType = tokenType;
            ScopeStore = scopeStore;
            ClientStore = clientStore;
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ClaimConverter());
            settings.Converters.Add(new ClaimsPrincipalConverter());
            settings.Converters.Add(new ClientConverter(ClientStore));
            settings.Converters.Add(new ScopeConverter(ScopeStore));
            return settings;
        }

        protected string ConvertToJson(T value)
        {
            return JsonConvert.SerializeObject(value, GetJsonSerializerSettings());
        }

        protected T ConvertFromJson(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetJsonSerializerSettings());
        }

        public async Task<T> GetAsync(string key)
        {
            var token = await GetStoredToken(key);

            if (token == null || token.Expires < DateTimeOffset.UtcNow)
            {
                return null;
            }

            return ConvertFromJson(token.SerializedToken);
        }

        public async Task RemoveAsync(string key)
        {
            using (var connection = new NpgsqlConnection(PostgreSqlServiceOptions.ConnectionString))
            {
                var command = connection.CreateCommand();

                command.CommandText = "delete from tokens where key = @key";

                command.Parameters.Add(new NpgsqlParameter
                {
                    ParameterName = "key",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = key
                });

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<ITokenMetadata>> GetAllAsync(string subject)
        {
            const string sql = @"
                select
                    key,
                    token_type,
                    subjectid,
                    clientid,
                    expires,
                    token,
                from 
                    tokens
                where
                    subject = @subject,
                    token_type = @tokenType
            ";

            using (var connection = new NpgsqlConnection(PostgreSqlServiceOptions.ConnectionString))
            {
                var storedTokens = await connection.QueryAsync<StoredToken>(sql, new { subject, tokenType = TokenType });

                var results = storedTokens.Select(x => ConvertFromJson(x.SerializedToken)).ToArray();
                return results.Cast<ITokenMetadata>();
            }
        }

        public async Task RevokeAsync(string subject, string client)
        {
            using (var connection = new NpgsqlConnection(PostgreSqlServiceOptions.ConnectionString))
            {
                var command = connection.CreateCommand();

                command.CommandText = "delete from tokens where subjectid = @subjectId and clientid = @clientId and token_type = @tokenType";

                command.Parameters.Add(new NpgsqlParameter
                {
                    ParameterName = "subjectId",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = subject
                });

                command.Parameters.Add(new NpgsqlParameter
                {
                    ParameterName = "clientId",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = client
                });

                command.Parameters.Add(new NpgsqlParameter
                {
                    ParameterName = "tokenType",
                    NpgsqlDbType = NpgsqlDbType.Varchar,
                    Value = TokenType
                });

                await command.ExecuteNonQueryAsync();
            }
        }

        public abstract Task StoreAsync(string key, T value);

        protected async Task<StoredToken> GetStoredToken(string key)
        {
            const string sql = @"
                select
                    key,
                    token_type,
                    subjectid,
                    clientid,
                    expires,
                    token
                from 
                    tokens
                where
                    key = @key and
                    token_type = @tokenType
            ";

            using (var connection = new NpgsqlConnection(PostgreSqlServiceOptions.ConnectionString))
            {
                return (await connection.QueryAsync<StoredToken>(sql, new {key, tokenType = TokenType.RefreshToken})).SingleOrDefault();
            }
        }
    }
}