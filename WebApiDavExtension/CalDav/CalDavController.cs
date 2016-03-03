using System;
using System.Collections.Generic;
using System.Linq;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
	public abstract class CalDavController : WebDavController
	{
        public override Resource LoadResource(string path)
        {
            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (uriSegments.Length > 3)
            {
                throw new Exception("Not found");
            }

            if (uriSegments.Length == 3)
            {
                return LoadEvent(uriSegments[0], uriSegments[1], uriSegments[2]);
            }

            if (uriSegments.Length == 2)
            {
                return LoadCalendar(uriSegments[0], uriSegments[1]);
            }

            return LoadPrincipal(uriSegments[0]);
        }

        public override IEnumerable<Resource> LoadCollectionResourceChildren(string path)
        {
            string[] uriSegments = path.Split('/');
            return LoadEventsFromCalendar(uriSegments[0], uriSegments[1]).Cast<Resource>().ToList();
        }

	    public override IEnumerable<Resource> QueryResources(string path, ReportRequest reportRequest)
	    {
	        string principalId;
	        string calendarId;
	        string eventId;

	        int found = GetIds(path, out principalId, out calendarId, out eventId);

            if (found >= 2)
	        {
                return CalendarQuery(principalId, calendarId, reportRequest);
            }

            throw new InvalidOperationException("Not found");
        }

	    public override IEnumerable<Resource> MultigetResources(string path, ReportRequest reportRequest)
	    {
            string principalId;
            string calendarId;
            string eventId;

            int found = GetIds(path, out principalId, out calendarId, out eventId);

            if (found >= 2)
            {
                return CalendarMultiget(principalId, calendarId, reportRequest);
            }

            throw new InvalidOperationException("Not found");
        }

	    private int GetIds(string path, out string principalId, out string calendarId, out string eventId)
	    {
	        principalId = string.Empty;
	        calendarId = string.Empty;
	        eventId = string.Empty;

	        int result = 0;
            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

	        if (uriSegments.Length > 0)
	        {
	            principalId = uriSegments[0];
	            result = 1;
	        }

	        if (uriSegments.Length > 1)
	        {
	            calendarId = uriSegments[1];
                result = 2;
            }

	        if (uriSegments.Length > 2)
	        {
	            eventId = uriSegments[2];
                result = 3;
            }

	        return result;
	    }

	    public virtual IEnumerable<CalendarResource> CalendarQuery(string principalId, string calendarId, ReportRequest reportRequest)
	    {
	        List<CalendarResource> events = new List<CalendarResource>();

            if (reportRequest.TimeRangeFilter != null)
            {
                events.AddRange(GetEventsByTimeRange(
                    principalId, calendarId, reportRequest.TimeRangeFilter.Start, reportRequest.TimeRangeFilter.End));
            }

            if (reportRequest.TextMatchFilter != null)
            {
                events.AddRange(GetEventsByTextMatch(principalId, calendarId, reportRequest.TextMatchFilter.Value,
                    reportRequest.TextMatchFilter.NegateCondition));
            }

	        //if (reportRequest.ParamFilter != null)
	        //{
	        //    events.AddRange();
	        //}

	        return events;
	    }

        /// <summary>
        /// Load the principal with the requested id
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <returns>the principal collection</returns>
        public abstract CalendarCollection LoadPrincipal(string principalId);

        /// <summary>
        /// Load the requested calendar collection
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <returns>the requested calendar collection</returns>
        public abstract CalendarCollection LoadCalendar(string principalId, string calendarId);

        /// <summary>
        /// Load the event
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="eventId">the id of the event</param>
        /// <returns>the requectes event resource</returns>
        public abstract CalendarResource LoadEvent(string principalId, string calendarId, string eventId);

        /// <summary>
        /// Load all events from calendar
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <returns></returns>
        public abstract IEnumerable<CalendarResource> LoadEventsFromCalendar(string principalId, string calendarId);

        /// <summary>
        /// Get multiple calendar resources
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="multigetReportRequest">the multiget request</param>
        /// <returns>enumerable list of all requested calendar resources</returns>
        public abstract IEnumerable<CalendarResource> CalendarMultiget(string principalId, string calendarId, ReportRequest multigetReportRequest);

        /// <summary>
        /// Get all events within given time range
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="start">the start date</param>
        /// <param name="end">the end date</param>
        /// <returns>all event resources in the given time range</returns>
        public abstract IEnumerable<CalendarResource> GetEventsByTimeRange(
            string principalId, string calendarId, DateTime start, DateTime end);

        /// <summary>
        /// Find events by text match
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="searchText">the text to match against</param>
        /// <param name="negateCondition">is the condition negated</param>
        /// <returns>enumerable list of all found event resources</returns>
        public abstract IEnumerable<CalendarResource> GetEventsByTextMatch(
            string principalId, string calendarId, string searchText, bool negateCondition);

	}
}
