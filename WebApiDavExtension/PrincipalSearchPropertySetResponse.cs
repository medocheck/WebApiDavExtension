using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;

namespace WebApiDavExtension
{
    public class PrincipalSearchPropertySetResponse : IHttpActionResult
    {
        private readonly HttpRequestMessage _request;

        public List<Tuple<string, string, string>> Properties { get; } = new List<Tuple<string, string, string>>();

        public PrincipalSearchPropertySetResponse(HttpRequestMessage request)
        {
            _request = request;
        }

        public PrincipalSearchPropertySetResponse(HttpRequestMessage request, IEnumerable<Tuple<string, string, string>> properties)
        {
            _request = request;
            Properties.AddRange(properties);
        }

        public string GetResponseData()
        {
            var principalSearchPropertySetStatusElement = new XElement(Namespaces.Dav + "principal-search-property-set");
            XDocument document = new XDocument(principalSearchPropertySetStatusElement);

            foreach (var property in Properties)
            {
                XElement element = new XElement(Namespaces.Dav + "principal-search-property", 
                    new XElement(Namespaces.Dav + "prop", 
                        new XElement(XNamespace.Get(property.Item1) + property.Item2)),
                    new XElement(Namespaces.Dav + "description", property.Item3));

                principalSearchPropertySetStatusElement.Add(element);
            }

            return document.ToString();
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(GetResponseData()),
                RequestMessage = _request
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            return Task.FromResult(response);
        }
    }
}
