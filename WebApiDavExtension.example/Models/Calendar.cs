using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using WebApiDavExtension.CalDav;
using WebApiDavExtension.Configuration;
using WebApiDavExtension.WebDav;
using CalendarComponent = WebApiDavExtension.Configuration.CalendarComponent;

namespace WebApiDavExtension.example.Models
{
	public class Calendar : ICalendarCollection
	{
        public Calendar()
        {
            System.TimeZoneInfo timezoneinfo = System.TimeZoneInfo.Local;
            Timezone = iCalTimeZone.FromSystemTimeZone(timezoneinfo);
            SupportedReportSet = ServerConfiguration.SupportedReportSet;
        }

        private CalDavConfiguration ServerConfiguration => (CalDavConfiguration)ConfigurationManager.GetSection("calDavConfiguration");

        public HRef CurrentUserPrincipal { get; set; }

        public HRef Owner { get; set; }

        public object CTag { get; set; }

        public XElement[] ResourceType => new[] { new XElement(XNamespace.Get("DAV:") + "collection"), new XElement(XNamespace.Get("urn:ietf:params:xml:ns:caldav") + "calendar") };

        public List<CalendarComponent> SupportedCalendarComponentSet => ServerConfiguration.SupportedCalendarComponents;

        public List<ReportType> SupportedReportSet { get; }

        public XElement CurrentUserPrivilegeSet => ServerConfiguration.CurrentUserPrivilegeSet;

        public string CalendarColor { get; set; }

        public string TimezoneString
        {
            get
            {
                if (Timezone == null)
                {
                    return string.Empty;
                }

                var calendar = new iCalendar();
                calendar.AddTimeZone(Timezone);

                iCalendarSerializer serializer = new iCalendarSerializer(calendar);
                string result = serializer.SerializeToString(calendar);

                return result;
            }

            set
            {
                iCalendarSerializer serializer = new iCalendarSerializer();
                var calendars = (iCalendarCollection)serializer.Deserialize(new StringReader(value));

                Timezone = calendars.First().TimeZones.First();
            }
        }

        public string CalendarId { get; set; }

        public string PrincipalId { get; set; }

		[PropFind("displayname", true, Namespace = "DAV:")]
		public string DisplayName { get; set; }

		[PropFind("calendar-description", true)]
		public string CalendarDescription { get; set; }

		public List<Event> Events { get; } = new List<Event>();

        public ITimeZone Timezone { get; set; }
        public string HRef { get; set; }
        public object ETag { get; set; }
        public MemoryStream GetOutputData()
        {
            throw new System.NotImplementedException();
        }

        public List<IDavResource> Resources { get; } = new List<IDavResource>();
    }
}
