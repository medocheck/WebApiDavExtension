using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using System.Xml.Linq;

namespace WebApiDavExtension
{
    [ModelBinder(typeof(PropertyUpdateRequestModelBinder))]
    public class PropertyUpdateRequest : Request
    {
        public Dictionary<XName, string> PropertiesToUpdate { get; } = new Dictionary<XName, string>(); 
    }
}
