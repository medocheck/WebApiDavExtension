using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Xml.Linq;

namespace WebApiDavExtension
{
	public class MkCalendarRequestModelBinder : IModelBinder
	{
		public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
		{
			if (bindingContext.ModelType != typeof(MkCalendarRequest))
			{
				return false;
			}

			ValueProviderResult val = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

			if (val == null)
			{
				return false;
			}

			XDocument document = val.RawValue as XDocument;

			if (document == null)
			{
				bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Wrong value type");
				return false;
			}

			MkCalendarRequest mkCalendar = new MkCalendarRequest();

			if (document.Descendants(Namespaces.Dav + "displayname").Any())
			{
				mkCalendar.CalendarName = document.Descendants(Namespaces.Dav + "displayname").First().Value;
			}

			if (document.Descendants(Namespaces.Caldav +  "calendar-description").Any())
			{
				mkCalendar.CalendarDescription = document.Descendants(Namespaces.Caldav + "calendar-description").First().Value;
			}

			if (document.Descendants(Namespaces.Caldav + "supported-calendar-component-set").Any())
			{
				foreach (
					var comp in document.Descendants(Namespaces.Caldav + "supported-calendar-component-set").First().Descendants(Namespaces.Caldav + "comp"))
				{
					mkCalendar.SupportedComponents.Add(comp.Attribute("name").Value);
				}
			}

			if (document.Descendants(Namespaces.Caldav + "calendar-timezone").Any())
			{
				var timezone = document.Descendants(Namespaces.Caldav + "calendar-timezone").First();
				mkCalendar.Timezone = timezone.Value;
			}

			bindingContext.Model = mkCalendar;
			return true;
		}
	}
}
