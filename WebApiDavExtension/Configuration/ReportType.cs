using System.Xml.Linq;
using System.Xml.Serialization;

namespace WebApiDavExtension.Configuration
{
    [XmlType("supported-report")]
    public class ReportType
    {
        [XmlElement("report")]
        public XElement ReportName { get; set; }
    }
}