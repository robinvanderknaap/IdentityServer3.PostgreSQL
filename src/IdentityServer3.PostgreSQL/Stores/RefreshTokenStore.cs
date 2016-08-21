using System.Threading.Tasks;
using Dapper;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.PostgreSQL.Models;
using Npgsql;

namespace IdentityServer3.PostgreSQL.Stores
{
    public class RefreshTokenStore : BaseTokenStore<RefreshToken>, IRefreshTokenStore
    {
        public RefreshTokenStore(PostgreSqlServiceOptions postgreSqlServiceOptions, IScopeStore scopeStore, IClientStore clientStore)
            : base(postgreSqlServiceOptions, TokenType.RefreshToken, scopeStore, clientStore)
        {
        }

        public override async Task StoreAsync(string key, RefreshToken value)
        {
            var storedToken = await GetStoredToken(key);

            if (storedToken != null)
            {
                const string sql = @"
                    update tokens set 
                        token = @Token,
                        expires = @Expires
                    where
                        key = @Key and
                        token_type = @TokenType
                ";

                using (var connection = new NpgsqlConnection(PostgreSqlServiceOptions.ConnectionString))
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        Token = ConvertToJson(value),
                        Expires = value.CreationTime.AddSeconds(value.LifeTime)
                    });
                }
            }
            else
            {
                const string sql = @"
                    insert into tokens 
                    ( 
                        key,
                        subjectid,
                        clientid,
                        token_type,
                        token,
                        expires
                    )
                    values
                    (
                        @Key,
                        @SubjectId,
                        @ClientId,
                        @TokenType,   
                        @Token,
                        @Expires
                    )
                ";

                using (var connection = new NpgsqlConnection(PostgreSqlServiceOptions.ConnectionString))
                {
                    await connection.ExecuteAsync(sql, new
                    {
                        Key = key,
                        value.SubjectId,
                        value.ClientId,
                        TokenType,
                        Token = ConvertToJson(value),
                        Expires = value.CreationTime.AddSeconds(value.LifeTime)
                    });
                }
            }
        }
    }
}