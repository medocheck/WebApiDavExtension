using System;
using System.Collections.Generic;
using System.Linq;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
    public class CalDavPathIsEmptyException : Exception
    {
        
    }

    public abstract class CalDavPrincipalController : WebDavController
    {
        public override IDavResource LoadResource(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new CalDavPathIsEmptyException();
            }

            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (uriSegments.Length == 0)
            {
                throw new Exception("No Principal Id submitted");
            }

            return LoadPrincipal(uriSegments[0]);
        }

        public override IEnumerable<IDavResource> LoadCollectionResourceChildren(string path)
        {
            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (uriSegments.Length == 0)
            {
                throw new Exception("No Principal Id submitted");
            }

            return LoadCalendar(uriSegments[0]);
        }

        public override IEnumerable<IDavResource> QueryResources(string path, ReportRequest reportRequest)
        {
            return new [] { LoadResource(path) };
        }

        public override IEnumerable<IDavResource> MultigetResources(string path, ReportRequest reportRequest)
        {
            var results = new List<IDavResource>();

            foreach (var hRef in reportRequest.HRefs)
            {
                string principalId, calendarId;

                string calendarHRef = hRef.Replace($"{ServerConfiguration.HRef}/Calendar/", "");
                GetIds(calendarHRef, out principalId, out calendarId);

                if (hRef.Contains($"{ServerConfiguration.HRef}/Calendar/"))
                {
                    var calendarResource = LoadCalendar(principalId, calendarId);
                    var events = LoadEvents(principalId, calendarId);

                    calendarResource.Resources.AddRange(events);

                    results.Add(calendarResource);
                }
                else
                {
                    var principalResource = LoadPrincipal(principalId);
                    results.Add(principalResource);
                }
            }

            return results;
        }

        public abstract IDavPrincipal LoadPrincipal(string principalId);

        public abstract IEnumerable<ICalendarCollection> LoadCalendar(string principalId);

        public abstract ICalendarCollection LoadCalendar(string principalId, string calendarId);

        public abstract IEnumerable<ICalendarResource> LoadEvents(string principalId, string calendarId);
    }
}
