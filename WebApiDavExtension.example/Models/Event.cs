using System.IO;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension.example.Models
{
	public class Event : CalendarResource
	{
		[PropFind("getcontenttype", Namespace = "DAV:")]
		public string ContentType { get; } = "text/calendar; charset=utf-8";

		public DDay.iCal.Event CalendarData { get; set; } = new DDay.iCal.Event();

		[PropFind("calendar-data", Namespace = "urn:ietf:params:xml:ns:caldav")]
		public string CalendarDataString
		{
			get
			{
				if (CalendarData == null)
				{
					return string.Empty;
				}

				var calendar = new iCalendar();
				calendar.Events.Add(CalendarData);

				iCalendarSerializer serializer = new iCalendarSerializer(calendar);
				string result = serializer.SerializeToString(calendar);

				return result;
			}
		}

        public override MemoryStream GetOutputData()
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CalendarDataString));
            return stream;
        }
    }
}
