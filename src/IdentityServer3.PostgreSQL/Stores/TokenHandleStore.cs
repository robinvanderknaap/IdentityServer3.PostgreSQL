﻿using System;
using System.Threading.Tasks;
using Dapper;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.PostgreSQL.Models;
using Npgsql;

namespace IdentityServer3.PostgreSQL.Stores
{
    public class TokenHandleStore : BaseTokenStore<Token>, ITokenHandleStore
    {
        public TokenHandleStore(PostgreSqlServiceOptions postgreSqlServiceOptions, IScopeStore scopeStore, IClientStore clientStore)
            : base(postgreSqlServiceOptions, TokenType.TokenHandle, scopeStore, clientStore)
        {
        }

        public override async Task StoreAsync(string key, Token value)
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
                    Expires = DateTimeOffset.UtcNow.AddSeconds(value.Client.AuthorizationCodeLifetime)
                });
            }
        }
    }
}