using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;

namespace IdentityServer3.PostgreSQL.WebHost.Config
{
    public class StaticUserService : UserServiceBase
    {
        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            context.AuthenticateResult = new AuthenticateResult(context.UserName, context.UserName);

            await Task.FromResult(0);
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims = context.Subject.Claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            
            await Task.FromResult(0);

        }
    }
}