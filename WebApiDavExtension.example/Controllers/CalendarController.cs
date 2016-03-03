﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
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

        public override CalendarCollection LoadPrincipal(string principalId)
        {
            return _repository.GetPrincipalCalendars(principalId).FirstOrDefault();
        }

        public override CalendarCollection LoadCalendar(string principalId, string calendarId)
        {
            return _repository.GetCalendar(principalId, calendarId);
        }

        public override CalendarResource LoadEvent(string principalId, string calendarId, string eventId)
        {
            return _repository.GetEventById(principalId, calendarId, eventId);
        }

        public override IEnumerable<CalendarResource> LoadEventsFromCalendar(string principalId, string calendarId)
        {
            return _repository.GetEvents(calendarId, principalId);
        }

		public override IEnumerable<CalendarResource> CalendarMultiget(string principalId, string calendarId, ReportRequest multigetReportRequest)
		{
		    var events = _repository.GetEvents(calendarId, principalId);
		    return multigetReportRequest.HRefs.Select(hRef => events.FirstOrDefault(e => e.HRef == hRef)).Where(resultEvent => resultEvent != null).ToList();
		}

        public override IEnumerable<CalendarResource> GetEventsByTextMatch(string principalId, string calendarId, string searchText, bool negateCondition)
        {
            return _repository.GetEvents(calendarId, principalId);
        }

        public override IEnumerable<CalendarResource> GetEventsByTimeRange(string principalId, string calendarId, DateTime start, DateTime end)
        {
            return _repository.GetEvents(calendarId, principalId);
        } 
	}
}
