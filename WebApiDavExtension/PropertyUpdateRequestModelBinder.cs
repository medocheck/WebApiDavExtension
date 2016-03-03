using System.Linq;
using System.Xml.Linq;

namespace WebApiDavExtension
{
    public class PropertyUpdateRequestModelBinder : CalDavModelBinder<PropertyUpdateRequest>
    {
        internal override bool CreateModel(XDocument document)
        {
            if (document.Root == null)
            {
                return false;
            }

            foreach (XElement setElement in document.Root.Elements(Namespaces.Dav + "set"))
            {
                foreach (XElement propElement in setElement.Elements(Namespaces.Dav + "prop"))
                {
                    XElement valueElement = propElement.Elements().First();

                    XName propName = valueElement.Name;
                    string value = valueElement.Value;

                    Model.PropertiesToUpdate.Add(propName, value);
                }
            }

            return true;
        }
    }
}
