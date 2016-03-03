﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebApiDavExtension.Authentication
{
    public abstract class AuthorizationFilterAttributeBase : AuthorizationFilterAttribute
    {
        private readonly bool _issueChallenge;

        protected AuthorizationFilterAttributeBase(bool issueChallenge)
        {
            _issueChallenge = issueChallenge;
        }

        private static bool IsAuthenticated => HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated;

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (IsAuthenticated)
                return;

            var userName = GetAuthenticatedUser(actionContext);
            if (string.IsNullOrWhiteSpace(userName))
            {
                Challenge(actionContext);
                return;
            }

            var isUserAuthorized = IsUserAuthorized(userName);
            if (!isUserAuthorized)
            {
                Challenge(actionContext);
                return;
            }

            SetIdentity(actionContext, userName);
        }

        protected virtual void SetIdentity(HttpActionContext actionContext, string userName)
        {
            var identity = new GenericIdentity(userName);
            var principal = new GenericPrincipal(identity, new string[0]);

            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
                HttpContext.Current.User = principal;

            var context = actionContext.Request.GetRequestContext();
            context.Principal = principal;
        }

        protected abstract string GetAuthenticatedUser(HttpActionContext actionContext);

        protected abstract bool IsUserAuthorized(string userName);

        protected abstract AuthenticationHeaderValue GetUnauthorizedResponseHeader(HttpActionContext actionContext);

        private void Challenge(HttpActionContext actionContext)
        {
            if (!_issueChallenge)
                return;

            var headerValue = GetUnauthorizedResponseHeader(actionContext);
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.Headers.WwwAuthenticate.Add(headerValue);
            actionContext.Response = response;
        }
    }
}
