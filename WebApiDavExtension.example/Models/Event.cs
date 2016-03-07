using System.IO;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension.example.Models
{
	public class Event : ICalendarResource
	{
		public DDay.iCal.Event CalendarData { get; set; } = new DDay.iCal.Event();

        public string ContentType { get; } = "text/calendar; charset=utf-8";

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

	    public string HRef { get; set; }
	    public object ETag { get; set; }

	    public MemoryStream GetOutputData()
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(CalendarDataString));
            return stream;
        }
    }
}
