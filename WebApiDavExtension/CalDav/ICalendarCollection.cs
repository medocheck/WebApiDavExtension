using System.Collections.Generic;
using System.Xml.Linq;
using WebApiDavExtension.Configuration;
using WebApiDavExtension.WebDav;
using CalendarComponent = WebApiDavExtension.Configuration.CalendarComponent;

namespace WebApiDavExtension.CalDav
{
    public interface ICalendarCollection : IDavCollectionResource
    {
        string ExternalId { get; set; }

        [PropFind("current-user-principal", Namespace = "DAV:", IsComplex = true)]
		HRef CurrentUserPrincipal { get; set; }

		[PropFind("owner", Namespace = "DAV:", IsComplex = true)]
		HRef Owner { get; set; }

		[PropFind("getctag", Namespace = "http://calendarserver.org/ns/")]
		object CTag { get; set; }

		[PropFind("resourcetype", Namespace = "DAV:", IsList = true)]
		XElement[] ResourceType { get; }

		[PropFind("supported-calendar-component-set", Namespace = "urn:ietf:params:xml:ns:caldav", IsList = true)]
		List<CalendarComponent> SupportedCalendarComponentSet { get; }

		[PropFind("supported-report-set", Namespace = "DAV:", IsList = true)]
		List<ReportType> SupportedReportSet { get; }

	    [PropFind("current-user-privilege-set", Namespace = "DAV:")]
	    XElement CurrentUserPrivilegeSet { get; }

        [PropFind("calendar-color", Namespace = "http://apple.com/ns/ical/")]
        string CalendarColor { get; set; }

        [PropFind("calendar-timezone", Namespace = "urn:ietf:params:xml:ns:caldav")]
        string TimezoneString { get; }
    }
}
