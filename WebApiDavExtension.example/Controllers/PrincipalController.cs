using System;
using System.Collections.Generic;
using System.Web.Http;
using WebApiDavExtension.Authentication;
using WebApiDavExtension.CalDav;
using WebApiDavExtension.example.Models;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.example.Controllers
{
    [BasicAuthorizationFilter] // Enable Basic authentication for this controller.
    [DigestAuthorizationFilter]
    [Authorize] // Require authenticated requests.
    public class PrincipalController : CalDavPrincipalController
    {
        public override IEnumerable<ICalendarCollection> LoadCalendar(string principalId)
        {
            throw new NotImplementedException();
        }

        public override ICalendarCollection LoadCalendar(string principalId, string calendarId)
        {
            throw new NotImplementedException();
        }

        public override string LoadCurrentSyncToken(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ICalendarResource> LoadEvents(string principalId, string calendarId)
        {
            throw new NotImplementedException();
        }

        public override IDavPrincipal LoadPrincipal(string principalId)
        {
            return new Principal("/WebDavPrototype/api/Principals/" + principalId, "/WebDavPrototype/api/Calendar/" + principalId);
        }

        public override IEnumerable<IDavResource> LoadResourcesBySyncToken(string path, string token)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveResourse(string path)
        {
            throw new System.NotImplementedException();
        }
    }
}
