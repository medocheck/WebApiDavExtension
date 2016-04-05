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
	public class MultiStatusResponse : IHttpActionResult
	{
		private readonly HttpRequestMessage _request;
	    private readonly string _syncToken;

		public List<Response> Responses { get; } = new List<Response>();

		public MultiStatusResponse(HttpRequestMessage request)
		{
			_request = request;
		}

		public MultiStatusResponse(List<Response> responses, HttpRequestMessage request)
		{
			_request = request;
			Responses.AddRange(responses);
		}

        public MultiStatusResponse(string syncToken, List<Response> responses, HttpRequestMessage request)
        {
            _request = request;
            _syncToken = syncToken;
            Responses.AddRange(responses);
        }

        public string GetResponseData()
		{
			var multiStatusElement = new XElement(Namespaces.Dav + "multistatus");
			XDocument document = new XDocument(multiStatusElement);

			foreach (var response in Responses)
			{
				multiStatusElement.Add(response.ResponseElement);
			}

            if (!string.IsNullOrEmpty(_syncToken))
            {
                multiStatusElement.Add(new XElement(Namespaces.Dav + "sync-token", _syncToken));
            }

			return document.ToString();
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage((HttpStatusCode)207)
			{
				Content = new StringContent(GetResponseData()),
				RequestMessage = _request
			};
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

			return Task.FromResult(response);
		}
	}
}
