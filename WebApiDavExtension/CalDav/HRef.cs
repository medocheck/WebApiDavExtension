using System.Xml.Serialization;

namespace WebApiDavExtension.CalDav
{
    [XmlRoot("href", Namespace = "DAV:")]
    public class HRef
    {
        public HRef()
        {
        }

        public HRef(string reference)
        {
            Reference = reference;
        }

        [XmlText]
        public string Reference { get; set; }
    }
}
