using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.PostgreSQL.Extensions;
using IdentityServer3.PostgreSQL.SchemaGenerator;
using IdentityServer3.PostgreSQL.WebHost.Config;
using Microsoft.Owin.Cors;
using Owin;

namespace IdentityServer3.PostgreSQL.WebHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);

            var factory = new IdentityServerServiceFactory
            {
                ClientStore = new Registration<IClientStore, StaticClientStore>(),
                ScopeStore = new Registration<IScopeStore, StaticScopeStore>(),
                UserService = new Registration<IUserService, StaticUserService>()
            };

            factory.RegisterOperationalServices(new PostgreSqlServiceOptions
            {
                ConnectionString = "Server=127.0.0.1;Port=5432;Database=IdentityServer_PostgresSQL;User Id=postgres;Password=postgres;",
                SchemaCreationOptions = SchemaCreationOptions.AlwaysRecreate
            });


            app.UseIdentityServer(new IdentityServerOptions
            {
                SiteName = "PostgreSQL Authorization Server",
                Factory = factory,
                SigningCertificate = Certificate.Get(),
                RequireSsl = false
            });
        }
    }
}