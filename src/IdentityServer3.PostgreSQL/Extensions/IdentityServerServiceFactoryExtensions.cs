using System;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.PostgreSQL.SchemaGenerator;
using IdentityServer3.PostgreSQL.Stores;

namespace IdentityServer3.PostgreSQL.Extensions
{
    public static class IdentityServerServiceFactoryExtensions
    {
        public static void RegisterOperationalServices(this IdentityServerServiceFactory factory, PostgreSqlServiceOptions options)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.SchemaCreationOptions != SchemaCreationOptions.NeverCreate)
            {
                new SchemaGenerator.SchemaGenerator(options.ConnectionString).GenerateSchema(options.SchemaCreationOptions == SchemaCreationOptions.AlwaysRecreate);
            }

            factory.Register(new Registration<PostgreSqlServiceOptions>(resolver => options));
            factory.AuthorizationCodeStore = new Registration<IAuthorizationCodeStore, AuthorizationCodeStore>();
            factory.TokenHandleStore = new Registration<ITokenHandleStore, TokenHandleStore>();
            //factory.ConsentStore = new Registration<IConsentStore, ConsentStore>();
            factory.RefreshTokenStore = new Registration<IRefreshTokenStore, RefreshTokenStore>();
        }
    }
}
