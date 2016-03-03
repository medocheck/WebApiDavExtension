using System.Configuration;
using System.Xml;

namespace WebApiDavExtension.Configuration
{
	public class CalDavConfigurationSectionHandler : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			return CalDavConfiguration.FromXml(section.OuterXml);
		}
	}
}
