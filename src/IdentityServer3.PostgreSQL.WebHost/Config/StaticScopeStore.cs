using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;

namespace IdentityServer3.PostgreSQL.WebHost.Config
{
    public class StaticScopeStore : IScopeStore
    {
        private readonly IEnumerable<Scope> _scopes;

        public StaticScopeStore()
        {
            _scopes = new List<Scope>
            {
                new Scope
                {
                    Name = "test",
                    Type = ScopeType.Resource
                },
                new Scope
                {
                    Name = "offline_access",
                    Type = ScopeType.Resource
                }
            };
        }

        public Task<IEnumerable<Scope>> FindScopesAsync(IEnumerable<string> scopeNames)
        {
            return Task.FromResult(_scopes);
        }

        public Task<IEnumerable<Scope>> GetScopesAsync(bool publicOnly = true)
        {
            return Task.FromResult(_scopes);
        }
    }
}