using System.Web.Http;
using WebApiDavExtension.Authentication;
using WebApiDavExtension.CalDav;
using WebApiDavExtension.example.Models;

namespace WebApiDavExtension.example.Controllers
{
    [BasicAuthorizationFilter] // Enable Basic authentication for this controller.
    [DigestAuthorizationFilter]
    [Authorize] // Require authenticated requests.
    public class PrincipalController : CalDavPrincipalController
    {
        public override IDavPrincipal LoadPrincipal(string principalId)
        {
            return new Principal("/WebDavPrototype/api/Principals/" + principalId, "/WebDavPrototype/api/Calendar/" + principalId);
        }

        public override bool RemoveResourse(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}
