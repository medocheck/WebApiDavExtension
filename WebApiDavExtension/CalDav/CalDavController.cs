using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web.Http;
using DDay.iCal;
using log4net;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
	public abstract class CalDavController : WebDavController
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [AcceptVerbs("MKCALENDAR")]
        public virtual IHttpActionResult MkCalendar(string path, MkCalendarRequest request)
	    {
            string principalId;
            string calendarId;

            int found = GetIds(path, out principalId, out calendarId);

            if (found < 2)
            {
                throw new InvalidOperationException("Calendar is missing");
            }

            bool success = AddCalendar(principalId, calendarId, request);

            if (!success)
            {
                return BadRequest("Could not save appointment");
            }

            return Ok();
        }

        [AcceptVerbs("PUT")]
        public virtual IHttpActionResult Put(string path, IICalendar resource)
        {
            Log.Debug("PUT \t HRef: " + path);

            bool success = AddResource(path, resource);

            if (!success)
            {
                return BadRequest("Could not save appointment");
            }

            return Created();
        }

        public bool AddResource(string path, IICalendar resource)
	    {
            string principalId;
            string calendarId;
            string eventId;

            int found = GetIds(path, out principalId, out calendarId, out eventId);

	        if (found < 2)
	        {
                throw new InvalidOperationException("Calendar is missing");
            }

            if (found > 2)
            {
                return AddEvent(principalId, calendarId, eventId, resource);
            }

            return AddEvent(principalId, calendarId, resource);
        }

	    public override IDavResource LoadResource(string path)
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

        public override IEnumerable<IDavResource> LoadCollectionResourceChildren(string path)
        {
            string[] uriSegments = path.Split('/');
            return LoadEventsFromCalendar(uriSegments[0], uriSegments[1]).Cast<IDavResource>().ToList();
        }

	    public override IEnumerable<IDavResource> QueryResources(string path, ReportRequest reportRequest)
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

	    public override IEnumerable<IDavResource> MultigetResources(string path, ReportRequest reportRequest)
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

	    public override bool RemoveResourse(string path)
	    {
            string principalId;
            string calendarId;
            string eventId;

            int found = GetIds(path, out principalId, out calendarId, out eventId);

            if (found >= 3)
            {
                return RemoveEvent(principalId, calendarId, eventId);
            }

            throw new InvalidOperationException("Not found");
        }

	    private int GetIds(string path, out string principalId, out string calendarId)
        {
            principalId = string.Empty;
            calendarId = string.Empty;

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

            return result;
        }

        private int GetIds(string path, out string principalId, out string calendarId, out string eventId)
	    {
	        eventId = string.Empty;

            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int result = GetIds(path, out principalId, out calendarId);

	        if (uriSegments.Length > 2)
	        {
	            eventId = uriSegments[2];

	            if (eventId.EndsWith(".ics"))
	            {
	                eventId = eventId.Substring(0, eventId.LastIndexOf(".ics", StringComparison.Ordinal));
	            }

                result = 3;
            }

	        return result;
	    }

	    public virtual IEnumerable<ICalendarResource> CalendarQuery(string principalId, string calendarId, ReportRequest reportRequest)
	    {
	        List<ICalendarResource> events = new List<ICalendarResource>();

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

	        return events;
	    }

	    public override string LoadCurrentSyncToken(string path)
	    {
            string principalId;
            string calendarId;

            int found = GetIds(path, out principalId, out calendarId);

	        if (found < 2)
	        {
	            throw new Exception("Calendar not found");
	        }

	        return LoadCurrentSyncToken(principalId, calendarId);
	    }

	    public override IEnumerable<IDavResource> LoadResourcesBySyncToken(string path, string token)
        {
            string principalId;
            string calendarId;

            int found = GetIds(path, out principalId, out calendarId);

            if (found < 2)
            {
                throw new Exception("Calendar not found");
            }

            return LoadResourcesBySyncToken(principalId, calendarId, token);
        }

        public abstract bool AddCalendar(string principalId, string calendarId, MkCalendarRequest request);

        public abstract bool AddEvent(string principalId, string calendarId, IICalendar resource);

        public abstract bool AddEvent(string principalId, string calendarId, string eventId, IICalendar resource);

        public abstract bool AddEvent(string principalId, string calendarId, ICalendarResource resource);

        /// <summary>
        /// Load the principal with the requested id
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <returns>the principal collection</returns>
        public abstract ICalendarCollection LoadPrincipal(string principalId);

        /// <summary>
        /// Load the requested calendar collection
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <returns>the requested calendar collection</returns>
        public abstract ICalendarCollection LoadCalendar(string principalId, string calendarId);

        /// <summary>
        /// Load the event
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="eventId">the id of the event</param>
        /// <returns>the requectes event resource</returns>
        public abstract ICalendarResource LoadEvent(string principalId, string calendarId, string eventId);

        /// <summary>
        /// Load all events from calendar
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <returns></returns>
        public abstract IEnumerable<ICalendarResource> LoadEventsFromCalendar(string principalId, string calendarId);

        /// <summary>
        /// Get multiple calendar resources
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="multigetReportRequest">the multiget request</param>
        /// <returns>enumerable list of all requested calendar resources</returns>
        public abstract IEnumerable<ICalendarResource> CalendarMultiget(string principalId, string calendarId, ReportRequest multigetReportRequest);

        /// <summary>
        /// Get all events within given time range
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="start">the start date</param>
        /// <param name="end">the end date</param>
        /// <returns>all event resources in the given time range</returns>
        public abstract IEnumerable<ICalendarResource> GetEventsByTimeRange(
            string principalId, string calendarId, DateTime start, DateTime end);

        /// <summary>
        /// Find events by text match
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="searchText">the text to match against</param>
        /// <param name="negateCondition">is the condition negated</param>
        /// <returns>enumerable list of all found event resources</returns>
        public abstract IEnumerable<ICalendarResource> GetEventsByTextMatch(
            string principalId, string calendarId, string searchText, bool negateCondition);

        /// <summary>
        /// Get the current sync token
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <returns></returns>
	    public abstract string LoadCurrentSyncToken(string principalId, string calendarId);

        /// <summary>
        /// Get all changed events after the given sync token
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="token">the sync token</param>
        /// <returns></returns>
        public abstract IEnumerable<ICalendarResource> LoadResourcesBySyncToken(string principalId, string calendarId, string token);

        /// <summary>
        /// Delete an event
        /// </summary>
        /// <param name="principalId">the id of the principal</param>
        /// <param name="calendarId">the id of the calendar</param>
        /// <param name="eventId">the id of the event</param>
        /// <returns>true if event was deleted; otherwise false</returns>
        public abstract bool RemoveEvent(string principalId, string calendarId, string eventId);
    }
}
