using System;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;

namespace WebApiDavExtension.Authentication
{
    public abstract class DigestAuthorizationFilterAttributeBase : AuthorizationFilterAttributeBase
    {
        private const string Scheme = "Digest";

        protected DigestAuthorizationFilterAttributeBase(bool issueChallenge)
            : base(issueChallenge)
        {
        }

        protected override string GetAuthenticatedUser(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;
            if (auth == null || auth.Scheme != Scheme)
                return null;

            var header = DigestHeader.Create(
                actionContext.Request.Headers.Authorization.Parameter,
                actionContext.Request.Method.Method);

            if (!DigestNonce.IsValid(header.Nonce, header.NounceCounter))
                return null;

            var password = GetPassword(header.UserName);

            var hash1 = $"{header.UserName}:{header.Realm}:{password}".ToMd5Hash();

            var hash2 = $"{header.Method}:{header.Uri}".ToMd5Hash();

            var computedResponse =
                $"{hash1}:{header.Nonce}:{header.NounceCounter}:{header.Cnonce}:{"auth"}:{hash2}".ToMd5Hash();

            return header.Response.Equals(computedResponse, StringComparison.Ordinal)
                ? header.UserName
                : null;
        }

        protected abstract string GetPassword(string userName);

        protected override AuthenticationHeaderValue GetUnauthorizedResponseHeader(HttpActionContext actionContext)
        {
            var host = actionContext.Request.RequestUri.DnsSafeHost;
            var header = DigestHeader.Unauthorized(host);
            var parameter = header.ToString();
            return new AuthenticationHeaderValue(Scheme, parameter);
        }
    }
}
