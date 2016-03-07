using System;
using System.Collections.Generic;
using System.Linq;
using DDay.iCal;
using WebApiDavExtension.CalDav;
using WebApiDavExtension.example.Models;
using Event = WebApiDavExtension.example.Models.Event;

namespace WebApiDavExtension.example.Repository
{
	public class CalendarRepository
	{
        private static readonly List<Calendar> Calendars = new List<Calendar>
        {
            new Calendar
            {
                CalendarId = "standard",
                PrincipalId = "Maddin",
                CalendarDescription = "Not a real Calendar",
                DisplayName = "Test Calendar",
                HRef = "/WebDavPrototype/api/Calendar/Maddin/standard",
                Owner = new HRef("/WebDavPrototype/api/Principals/Maddin"),
                CurrentUserPrincipal = new HRef("/WebDavPrototype/api/Principals/Maddin"),
                CTag = Guid.NewGuid(),
                ETag = Guid.NewGuid(),
                Events =
                {
                    new Event
                    {
                        CalendarData = new DDay.iCal.Event
                        {
                            UID = "d2747d91-e340-4e69-9dbe-5dec7a217927.ics",
                            Summary = "Mein erster Event",
                            Start = new iCalDateTime(new DateTime(2016, 01, 28, 12, 0, 0)),
                            End = new iCalDateTime(new DateTime(2016, 01, 28, 14, 0, 0)),
                        },
                        ETag = "C1E16F24-C023-4C52-96D2-0503632509E0",
                        HRef = "/WebDavPrototype/api/Calendar/Maddin/standard/" + "d2747d91-e340-4e69-9dbe-5dec7a217927.ics"
                    },
                    new Event
                    {
                        CalendarData = new DDay.iCal.Event
                        {
                            UID = "c7cb8d6d-dd09-4c9e-bb84-0c09cb5b2be2.ics",
                            Summary = "Mein anderer Event",
                            Start = new iCalDateTime(new DateTime(2016, 02, 03, 12, 0, 0)),
                            End = new iCalDateTime(new DateTime(2016, 02, 03, 14, 0, 0)),
                        },
                        ETag = "683F7D45-BEAA-4202-A9CC-FAF62C430063",
                        HRef = "/WebDavPrototype/api/Calendar/Maddin/standard/" + "c7cb8d6d-dd09-4c9e-bb84-0c09cb5b2be2.ics"
                    },
                    new Event
                    {
                        CalendarData = new DDay.iCal.Event
                        {
                            UID = "103b4c85-af01-4ebc-8227-2a13cd1669a8.ics",
                            Summary = "Mein anderer Event",
                            Start = new iCalDateTime(new DateTime(2016, 02, 03, 16, 0, 0)),
                            End = new iCalDateTime(new DateTime(2016, 02, 03, 17, 30, 0)),
                        },
                        ETag = "E00EA016-880C-4928-92F9-C7DCD0F89396",
                        HRef = "/WebDavPrototype/api/Calendar/Maddin/standard/" + "103b4c85-af01-4ebc-8227-2a13cd1669a8.ics"
                    }
                }
            }
        };

	    public Principal GetPrincipal(string principalId)
	    {
	        return new Principal("/WebDavPrototype/api/Principals/Maddin", "/WebDavPrototype/api/Calendar/Maddin/standard")
	        {
	            DisplayName = "Maddin"
	        };
	    }

	    public List<Calendar> GetPrincipalCalendars(string principalId)
	    {
	        return Calendars.Where(c => c.PrincipalId.ToLower() == principalId.ToLower()).ToList();
	    }

		public Calendar GetCalendar(string principalId, string calendarId)
		{
		    return Calendars.FirstOrDefault(c => c.PrincipalId.ToLower() == principalId.ToLower() && c.CalendarId.ToLower() == calendarId.ToLower());
		}

		public Event GetEventById(string principalId, string calendarId, string id)
		{
		    return Calendars
                .Where(c => c.PrincipalId.ToLower() == principalId.ToLower() && c.CalendarId.ToLower() == calendarId.ToLower())
                .SelectMany(c => c.Events)
                .FirstOrDefault(e => e.CalendarData.UID == id);
		}

		public List<Event> GetEvents(string calendarId, string principalId)
		{
		    return Calendars
		        .Where(c => c.PrincipalId.ToLower() == principalId.ToLower() && c.CalendarId.ToLower() == calendarId.ToLower())
		        .SelectMany(c => c.Events).ToList();
		}
	}
}
