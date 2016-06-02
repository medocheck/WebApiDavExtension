using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension
{
    public class CalendarMediaFormatter : MediaTypeFormatter
    {
        public CalendarMediaFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/calendar"));
        }

        public override bool CanReadType(Type type)
        {
            if (type == typeof(IICalendar))
            {
                return true;
            }

            Type enumerableType = typeof(IEnumerable<IICalendar>);
            return enumerableType.IsAssignableFrom(type);
        }

        public override bool CanWriteType(Type type)
        {
            if (type == typeof(IICalendar))
            {
                return true;
            }

            Type enumerableType = typeof(IEnumerable<IICalendar>);
            return enumerableType.IsAssignableFrom(type);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            string data = content.ReadAsStringAsync().Result;

            iCalendarSerializer serializer = new iCalendarSerializer();
            var calendars = (iCalendarCollection) serializer.Deserialize(new StringReader(data));

            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(calendars.FirstOrDefault());
            return tcs.Task;
        }
    }
}
