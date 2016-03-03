using System.Xml.Serialization;

namespace WebApiDavExtension.Configuration
{
    [XmlType("comp")]
    public class CalendarComponent
    {
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}