using System.Collections.Generic;
using System.Web.Http.ModelBinding;

namespace WebApiDavExtension
{
	[ModelBinder(typeof(MkCalendarRequestModelBinder))]
	public class MkCalendarRequest
	{
		public string CalendarName { get; set; }
		public string CalendarDescription { get; set; }
		public List<string> SupportedComponents { get; set; } = new List<string>(); 
		public string Timezone { get; set; }
	}
}
