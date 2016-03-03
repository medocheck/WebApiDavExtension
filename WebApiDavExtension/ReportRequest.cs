using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension
{
    [ModelBinder(typeof(ReportRequestModelBinder))]
	public class ReportRequest : Request
	{
		public  ReportRequestType Type { get; internal set; }

		public List<string> RequestedCalendarProperties { get; set; }
		public List<string> RequestedEventProperties { get; set; }
		public TimeRange TimeRangeFilter { get; set; }
		public TextMatch TextMatchFilter { get; set; }
		public ParamFilter ParamFilter { get; set; }
		public List<string> HRefs { get; } = new List<string>();
	}
}
