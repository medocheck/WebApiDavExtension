using System.Collections.Generic;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension.example.Models
{
	public class Calendar : CalendarCollection
	{
		public string CalendarId { get; set; }

        public string PrincipalId { get; set; }

		[PropFind("displayname", true, Namespace = "DAV:")]
		public string DisplayName { get; set; }

		[PropFind("calendar-description", true)]
		public string CalendarDescription { get; set; }

		public List<Event> Events { get; } = new List<Event>(); 
	}
}
