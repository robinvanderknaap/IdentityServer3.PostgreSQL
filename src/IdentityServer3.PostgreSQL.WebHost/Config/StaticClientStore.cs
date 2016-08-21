using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace IdentityServer3.PostgreSQL.WebHost.Config
{
    public class StaticClientStore : IClientStore
    {
        private readonly IList<Client> _clients;

        public StaticClientStore()
        {
            _clients = new List<Client>
            {
                new Client
                {
                    ClientId = "TestClient",
                    ClientName = "Test client",
                    ClientSecrets = new List<Secret> {new Secret("secret".Sha256())},
                    AllowedScopes = new List<string> { "test", "offline_access" },
                    AccessTokenType = AccessTokenType.Jwt,
                    Flow = Flows.ResourceOwner,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AbsoluteRefreshTokenLifetime = 2592000, // 2592000 = 30 days
                    SlidingRefreshTokenLifetime = 1296000, // 1296000 = 15 days
                    AccessTokenLifetime = 60 // 1800 = 0,5 hours
                },
                
            };
        }

        public Task<Client> FindClientByIdAsync(string clientId)
        {
            return Task.FromResult(_clients.SingleOrDefault(x => x.ClientId == clientId));
        }
    }
}