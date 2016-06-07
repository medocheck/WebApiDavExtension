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
                    : document.Root.Name.LocalName == "principal-search-property-set"
                        ? ReportRequestType.PrincipalSearchPropertySet
                        : ReportRequestType.SyncCollection;

            var eventFilter = from f in document.Descendants(Namespaces.Caldav + "comp-filter")
                              where f.Attribute("name").Value == "VEVENT"
                              select f;

            var elements = eventFilter as IList<XElement> ?? eventFilter.ToList();

            if (elements.Any())
            {
                CreateFilter(elements);
            }

		    if (Model.Type == ReportRequestType.SyncCollection)
		    {
		        HandleSyncReport(document);
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

	    private void HandleSyncReport(XDocument document)
	    {
	        var syncTokenElement = document.Descendants(Namespaces.Dav + "sync-token").FirstOrDefault();
	        var syncLevelElement = document.Descendants(Namespaces.Dav + "sync-level").FirstOrDefault();

	        if (syncTokenElement != null)
	        {
	            Model.SyncToken = syncTokenElement.Value;
	        }

	        if (syncLevelElement != null)
	        {
	            int syncLevel;

	            if (int.TryParse(syncLevelElement.Value, out syncLevel))
	            {
	                Model.SyncLevel = syncLevel;
	            }
	        }
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
	        var timerange = elements.Descendants(Namespaces.Caldav + "time-range").First();
			reportRequest.TimeRangeFilter = new TimeRange();

	        var startTime = timerange.Attribute("start").Value;

            //todo: refactor

	        if (!string.IsNullOrEmpty(startTime))
	        {
	            DateTime startDateTime;
	            if (DateTime.TryParseExact(timerange.Attribute("start").Value, "yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDateTime))
                {
                    reportRequest.TimeRangeFilter.Start = startDateTime;
                }
	        }
	        else
	        {
	            reportRequest.TimeRangeFilter.Start = DateTime.Now.Date;
	        }

	        var endTime = timerange.Attribute("end")?.Value;

	        if (!string.IsNullOrEmpty(endTime))
	        {
	            DateTime endDateTime;
	            if (DateTime.TryParseExact(endTime, "yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDateTime))
                {
                    reportRequest.TimeRangeFilter.End = endDateTime;
                }
	        }
	        else
	        {
                reportRequest.TimeRangeFilter.End = DateTime.MaxValue;
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
