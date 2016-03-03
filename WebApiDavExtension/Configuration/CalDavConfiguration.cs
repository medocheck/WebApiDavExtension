using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WebApiDavExtension.Configuration
{
	[XmlRoot("calDavConfiguration")]
	public class CalDavConfiguration
	{
		private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(CalDavConfiguration));

		[XmlAttribute("davHeader")]
		public string DavHeader { get; set; }

		[XmlArray("supported-calendar-component-set")]
		[XmlArrayItem("comp")]
		public List<CalendarComponent> SupportedCalendarComponents { get; set; } = new List<CalendarComponent>();

		[XmlArray("supported-report-set")]
		[XmlArrayItem("supported-report")]
		public List<ReportType> SupportedReportSet { get; set; } = new List<ReportType>();

		[XmlArray("allowed-http-methods")]
		[XmlArrayItem("method")]
		public List<string> AllowedHttpMethods { get; set; } = new List<string>();

        [XmlElement("global-privileges")]
        public XElement CurrentUserPrivilegeSet { get; set; }

        public static CalDavConfiguration FromXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				return new CalDavConfiguration();
			}

			return XmlSerializer.Deserialize(new StringReader(xml)) as CalDavConfiguration;
		}

		public string ToXml()
		{
			SupportedReportSet.Sort();

			using (var writer = new StringWriter())
			{
				writer.NewLine = Environment.NewLine;
				XmlSerializer.Serialize(writer, this);

				return writer.ToString();
			}
		}
	}
}
