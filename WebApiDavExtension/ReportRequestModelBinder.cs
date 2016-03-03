using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using WebApiDavExtension.CalDav;

namespace WebApiDavExtension
{
	public class ReportRequestModelBinder : CalDavModelBinder<ReportRequest>
	{
		internal override bool CreateModel(XDocument document)
		{
            if (document?.Root == null)
            {
                return false;
            }

            Model.Type = document.Root.Name.LocalName == "calendar-query"
                ? ReportRequestType.CalendarQuery
                : document.Root.Name.LocalName == "calendar-multiget"
                    ? ReportRequestType.Multiget
                    : ReportRequestType.PrincipalSearchPropertySet;

            var eventFilter = from f in document.Descendants(Namespaces.Caldav + "comp-filter")
                              where f.Attribute("name").Value == "VEVENT"
                              select f;

            var elements = eventFilter as IList<XElement> ?? eventFilter.ToList();

            if (elements.Any())
            {
                CreateFilter(elements);
            }

		    if (document.Root == null)
		    {
		        return false;
		    }

		    foreach (var element in document.Root.Elements(Namespaces.Dav + "href"))
		    {
		        Model.HRefs.Add(element.Value);
		    }

		    return true;
		}

	    private void CreateFilter(IList<XElement> elements)
	    {
	        if (elements.Descendants(Namespaces.Caldav + "time-range").Any())
	        {
	            CreateTimeRangeFilter(elements, Model);
	        }

	        if (elements.Descendants(Namespaces.Caldav + "prop-filter").Any())
	        {
	            CreatePropFilter(elements, Model);
	        }
	    }

	    private static void CreateTimeRangeFilter(IList<XElement> elements, ReportRequest reportRequest)
		{
			DateTime startDateTime;
			DateTime endDateTime;
			var timerange = elements.Descendants(Namespaces.Caldav + "time-range").First();
			reportRequest.TimeRangeFilter = new TimeRange();

			if (DateTime.TryParseExact(timerange.Attribute("start").Value, "yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture, DateTimeStyles.None,  out startDateTime))
			{
				reportRequest.TimeRangeFilter.Start = startDateTime;
			}

			if (DateTime.TryParseExact(timerange.Attribute("end").Value, "yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDateTime))
			{
				reportRequest.TimeRangeFilter.End = endDateTime;
			}
		}

		private void CreatePropFilter(IList<XElement> elements, ReportRequest reportRequest)
		{
			if (elements.Descendants(Namespaces.Caldav + "text-match").Any())
			{
				var textMatchNode = elements.Descendants(Namespaces.Caldav + "text-match").First();

				TextMatch textMatch = new TextMatch
				{
					Value = textMatchNode.Value
				};

				if (textMatchNode.Attribute("negate-condition") != null)
				{
					bool negateConditionValue;
					if (bool.TryParse(textMatchNode.Attribute("negate-condition").Value, out negateConditionValue))
					{
						textMatch.NegateCondition = negateConditionValue;
					}
				}

				reportRequest.TextMatchFilter = textMatch;
			}
		}
	}
}
