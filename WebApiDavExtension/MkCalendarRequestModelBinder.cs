using System.Linq;
using System.Xml.Linq;

namespace WebApiDavExtension
{
	public class MkCalendarRequestModelBinder : CalDavModelBinder<MkCalendarRequest>
    {
        internal override bool CreateModel(XDocument document)
        {
			if (document.Descendants(Namespaces.Dav + "displayname").Any())
			{
				Model.CalendarName = document.Descendants(Namespaces.Dav + "displayname").First().Value;
			}

			if (document.Descendants(Namespaces.Caldav +  "calendar-description").Any())
			{
                Model.CalendarDescription = document.Descendants(Namespaces.Caldav + "calendar-description").First().Value;
			}

			if (document.Descendants(Namespaces.Caldav + "supported-calendar-component-set").Any())
			{
				foreach (
					var comp in document.Descendants(Namespaces.Caldav + "supported-calendar-component-set").First().Descendants(Namespaces.Caldav + "comp"))
				{
                    Model.SupportedComponents.Add(comp.Attribute("name").Value);
				}
			}

			if (document.Descendants(Namespaces.Caldav + "calendar-timezone").Any())
			{
				var timezone = document.Descendants(Namespaces.Caldav + "calendar-timezone").First();
                Model.Timezone = timezone.Value;
			}

			return true;
		}
    }
}
