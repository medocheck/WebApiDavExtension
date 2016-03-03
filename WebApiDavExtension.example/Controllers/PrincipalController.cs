using System.Web.Http;
using WebApiDavExtension.Authentication;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension.example.Controllers
{
    [BasicAuthorizationFilter] // Enable Basic authentication for this controller.
    [DigestAuthorizationFilter]
    [Authorize] // Require authenticated requests.
    public class PrincipalController : CalDavPrincipalController
    {
        public override Principal LoadPrincipal(string principalId)
        {
            return new Principal("/WebDavPrototype/api/Principals/" + principalId, "/WebDavPrototype/api/Calendar/" + principalId);
        }
    }
}
