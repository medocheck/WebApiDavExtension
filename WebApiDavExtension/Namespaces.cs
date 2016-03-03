using System.Xml.Linq;

namespace WebApiDavExtension
{
	public static class Namespaces
	{
		public static XNamespace Dav => "DAV:";

		public static XNamespace Caldav => "urn:ietf:params:xml:ns:caldav";

	    public static XNamespace CalendarServer => "CS:http://calendarserver.org/ns/";
	}
}
