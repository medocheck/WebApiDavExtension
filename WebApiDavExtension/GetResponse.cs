using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension
{
    public class GetResponse : IHttpActionResult
    {
        public string HRef { get; set; }
        public HttpStatusCode Status { get; set; }

        private readonly IDavResource _responseItem;
        private readonly HttpRequestMessage _request;

        private readonly XDocument _collectionResponse;

        public GetResponse(string href, HttpRequestMessage request, IDavResource responseItem)
		{
            HRef = href;
            Status = HttpStatusCode.OK;

            _responseItem = responseItem;
            _request = request;

            _collectionResponse = new XDocument();
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_responseItem is IDavCollectionResource)
            {
                CreateCollectionResponse();
                return ExecuteCollectionAsync();
            }
            else
            {
                return ExecuteSingleResourceAsync();
            }

        }
        private void CreateCollectionResponse()
        {
            var collectionResource = _responseItem as IDavCollectionResource;
            var collectionElement = new XElement("collection");

            if (collectionResource != null)
            {
                foreach (IDavResource resource in collectionResource.Resources)
                {
                    var hrefAttribute = new XAttribute("href", resource.HRef);
                    var resourceElement = new XElement("resource", hrefAttribute);

                    collectionElement.Add(resourceElement);
                }
            }

            _collectionResponse.Add(collectionElement);
        }      

        private Task<HttpResponseMessage> ExecuteCollectionAsync()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_collectionResponse.ToString()),
                RequestMessage = _request
            };

            return Task.FromResult(response);
        }

        private Task<HttpResponseMessage> ExecuteSingleResourceAsync()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(_responseItem.GetOutputData())
            };

            var contentType = _responseItem.HRef != null 
                ? MimeMapping.GetMimeMapping(Path.GetExtension(_responseItem.HRef)) 
                : "application/octet-stream";

            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            return Task.FromResult(response);
        }
    }
}
