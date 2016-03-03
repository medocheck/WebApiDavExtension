using System.Collections.Generic;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension
{
    public abstract class Request
    {
        public RequestDepth RequestDepth { get; internal set; } = RequestDepth.Zero;
        public bool IsAllPropRequest { get; internal set; } = false;
        public List<string> RequestedPropList { get; } = new List<string>();

        public virtual Response CreateResponse(string href, Resource responseItem)
        {
            var response = new Response(href, responseItem, RequestedPropList);
            response.CreateXElement();

            return response;
        }
    }
}
