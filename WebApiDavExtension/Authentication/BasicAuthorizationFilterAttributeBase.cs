using System;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.Controllers;

namespace WebApiDavExtension.Authentication
{
    public abstract class BasicAuthorizationFilterAttributeBase : AuthorizationFilterAttributeBase
    {
        private const string Scheme = "Basic";

        protected BasicAuthorizationFilterAttributeBase(bool issueChallenge) 
            : base(issueChallenge)
        {
        }

        protected override string GetAuthenticatedUser(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            if (auth == null || auth.Scheme != Scheme)
                return null;

            var authHeader = auth.Parameter;
            if (string.IsNullOrEmpty(authHeader))
                return null;

            var authHeaderBytes = Convert.FromBase64String(authHeader);
            var decodedAuthHeader = Encoding.Default.GetString(authHeaderBytes);

            var tokens = decodedAuthHeader.Split(':');
            if (tokens.Length != 2)
                return null;

            return IsPasswordValid(tokens[0], tokens[1])
                ? tokens[0]
                : null;
        }

        protected abstract bool IsPasswordValid(string userName, string password);

        protected override AuthenticationHeaderValue GetUnauthorizedResponseHeader(HttpActionContext actionContext)
        {
            var host = actionContext.Request.RequestUri.DnsSafeHost;
            var parameter = string.Format("realm=\"{0}\"", host);
            return new AuthenticationHeaderValue(Scheme, parameter);
        }
    }
}
