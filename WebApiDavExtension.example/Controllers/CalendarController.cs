using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DDay.iCal;
using WebApiDavExtension.Authentication;
using WebApiDavExtension.CalDav;
using WebApiDavExtension.example.Repository;

namespace WebApiDavExtension.example.Controllers
{
    [BasicAuthorizationFilter] // Enable Basic authentication for this controller.
    [DigestAuthorizationFilter]
    [Authorize] // Require authenticated requests.
    public class CalendarController : CalDavController
	{
		readonly CalendarRepository _repository = new CalendarRepository();

        public override bool AddCalendar(string principalId, string calendarId, MkCalendarRequest request)
        {
            return true;
        }

        public override bool AddEvent(string principalId, string calendarId, IICalendar resource)
        {
            return true;
        }

        public override bool AddEvent(string principalId, string calendarId, string eventId, IICalendar resource)
        {
            return true;
        }

        public override ICalendarCollection LoadPrincipal(string principalId)
        {
            return _repository.GetPrincipalCalendars(principalId).FirstOrDefault();
        }

        public override ICalendarCollection LoadCalendar(string principalId, string calendarId)
        {
            return _repository.GetCalendar(principalId, calendarId);
        }

        public override bool AddEvent(string principalId, string calendarId, ICalendarResource resource)
        {
            return true;
        }

        public override ICalendarResource LoadEvent(string principalId, string calendarId, string eventId)
        {
            return _repository.GetEventById(principalId, calendarId, eventId);
        }

        public override IEnumerable<ICalendarResource> LoadEventsFromCalendar(string principalId, string calendarId)
        {
            return _repository.GetEvents(calendarId, principalId);
        }

		public override IEnumerable<ICalendarResource> CalendarMultiget(string principalId, string calendarId, ReportRequest multigetReportRequest)
		{
		    var events = _repository.GetEvents(calendarId, principalId);
		    return multigetReportRequest.HRefs.Select(hRef => events.FirstOrDefault(e => e.HRef == hRef)).Where(resultEvent => resultEvent != null).ToList();
		}

        public override IEnumerable<ICalendarResource> GetEventsByTextMatch(string principalId, string calendarId, string searchText, bool negateCondition)
        {
            return _repository.GetEvents(calendarId, principalId);
        }

        public override IEnumerable<ICalendarResource> GetEventsByTimeRange(string principalId, string calendarId, DateTime start, DateTime end)
        {
            return _repository.GetEvents(calendarId, principalId);
        }

        public override bool RemoveEvent(string principalId, string calendarId, string eventId)
        {
            throw new NotImplementedException();
        }

        public override string LoadCurrentSyncToken(string principalId, string calendarId)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ICalendarResource> LoadResourcesBySyncToken(string principalId, string calendarId, string token)
        {
            throw new NotImplementedException();
        }
    }
}
