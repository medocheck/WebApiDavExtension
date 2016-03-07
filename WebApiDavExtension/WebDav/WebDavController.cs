using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using WebApiDavExtension.Configuration;

namespace WebApiDavExtension.WebDav
{
    /// <summary>
    /// Handles all WebDav Requests
    /// </summary>
    public abstract class WebDavController : ApiController
    {
        protected CalDavConfiguration ServerConfiguration => (CalDavConfiguration)ConfigurationManager.GetSection("calDavConfiguration");

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [AcceptVerbs("OPTIONS")]
        public virtual HttpResponseMessage Options(string path)
        {
            Log.Debug("OPTIONS \t HRef: " + path);
            return Ok(ServerConfiguration.AllowedHttpMethods, ServerConfiguration.DavHeader);
        }

        /// <summary>
        /// Handles all PROPFIND requests
        /// </summary>
        /// <param name="path"></param>
        /// <param name="propFindRequest"></param>
        /// <returns></returns>
        [AcceptVerbs("PROPFIND")]
        public virtual IHttpActionResult Propfind(string path, PropFindRequest propFindRequest)
        {
            try
            {
                Log.Debug("PROPFIND \t HRef: " + path);

                foreach (var prop in propFindRequest.RequestedPropList)
                {
                    Log.Debug("\t requested property: " + prop);
                }

                List<Response> responses = new List<Response>();
                IDavResource resource = LoadResource(path);
                Response mainResponse = propFindRequest.CreateResponse(resource.HRef, resource);

                responses.Add(mainResponse);

                if (propFindRequest.RequestDepth != RequestDepth.Zero && resource is IDavCollectionResource)
                {
                    IEnumerable<IDavResource> eventList = LoadCollectionResourceChildren(path);
                    responses.AddRange(
                        eventList.Select(eventResource => propFindRequest.CreateResponse(eventResource.HRef, eventResource)));
                }

                return MultiStatus(responses);
            }
            catch (Exception exception)
            {
                Log.Error("Error in PROPFIND", exception);
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Handles all PROPPATCH requests
        /// </summary>
        /// <param name="path"></param>
        /// <param name="propertyUpdateRequest"></param>
        /// <returns></returns>
        [AcceptVerbs("PROPPATCH")]
        public virtual IHttpActionResult Proppatch(string path, PropertyUpdateRequest propertyUpdateRequest)
        {
            try
            {
                var resource = LoadResource(path);

                Response response = propertyUpdateRequest.CreateResponse(path, resource);

                foreach (var propertyToUpdatePair in propertyUpdateRequest.PropertiesToUpdate)
                {
                    if (!ResourcePropertyHelper.HasProperty(resource, propertyToUpdatePair.Key.LocalName))
                    {
                        response.AddForbiddenProperty(propertyToUpdatePair.Key.LocalName, propertyToUpdatePair.Key.NamespaceName);
                        continue;
                    }

                    if (ResourcePropertyHelper.UpdateProperty(resource, propertyToUpdatePair.Key.LocalName, propertyToUpdatePair.Value))
                    {
                        response.AddOkProperty(propertyToUpdatePair.Key.LocalName, propertyToUpdatePair.Key.NamespaceName);
                    }
                    else
                    {
                        response.AddForbiddenProperty(propertyToUpdatePair.Key.LocalName, propertyToUpdatePair.Key.NamespaceName);
                    }
                }

                return MultiStatus(new List<Response> { response });
            }
            catch (Exception exception)
            {
                Log.Error("Error in PROPPATCH", exception);
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Handles all REPORT requests
        /// </summary>
        /// <param name="path"></param>
        /// <param name="reportRequest"></param>
        /// <returns></returns>
        [AcceptVerbs("REPORT")]
        public virtual IHttpActionResult Report(string path, ReportRequest reportRequest)
        {
            try
            {
                Log.Debug("REPORT \t HRef: " + path);

                if (reportRequest.Type == ReportRequestType.PrincipalSearchPropertySet)
                {
                    return
                        PrincipalSearchPropertySet(
                            new Tuple<string, string, string>("DAV:", "displayname", "Display name"), new Tuple<string, string, string>("http://medocheck.com", "email-address", "Email address"));
                }

                if (path == null)
                {
                    Log.Warn("Empty Href in REPORT");
                    return NotFound();
                }

                LogReportRequest(reportRequest);

                IEnumerable<IDavResource> resources = reportRequest.Type == ReportRequestType.CalendarQuery
                    ? QueryResources(path, reportRequest)
                    : MultigetResources(path, reportRequest);

                List<Response> responses = resources.Select(resource => reportRequest.CreateResponse(resource.HRef, resource)).ToList();

                return MultiStatus(responses);
            }
            catch (Exception exception)
            {
                Log.Error("Error in REPORT", exception);
                return InternalServerError(exception);
            }
        }

        protected void LogReportRequest(ReportRequest reportRequest)
        {
            Log.Debug("\tType: " + reportRequest.Type);
            Log.Debug("\tRequestDepth: " + reportRequest.RequestDepth);
            Log.Debug("\tIsAllPropertiesRequested: " + reportRequest.IsAllPropRequest);

            Log.Debug("\tRequested Properties: ");

            foreach (string prop in reportRequest.RequestedPropList)
            {
                Log.Debug("\t\t" + prop);
            }

            Log.Debug("\tHRefs: ");

            foreach (var href in reportRequest.HRefs)
            {
                Log.Debug("\t\t" + href);
            }

            if (reportRequest.TimeRangeFilter != null)
            {
                Log.Debug("\tTimeRangeFilter:");
                Log.Debug("\t\t Start Date" + reportRequest.TimeRangeFilter.Start);
                Log.Debug("\t\t End Date" + reportRequest.TimeRangeFilter.End);
            }

            if (reportRequest.TextMatchFilter != null)
            {
                Log.Debug("\tTextMatchFilter:");
                Log.Debug("\t\t Value" + reportRequest.TextMatchFilter.Value);
            }

            if (reportRequest.ParamFilter != null)
            {
                Log.Debug("\tParamFilter");
            }
        }

        /// <summary>
        /// Handles all GET requests
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual IHttpActionResult Get(string path)
        {
            try
            {
                Log.Debug("GET \t HRef: " + path);

                var resource = LoadResource(path);
                return new GetResponse(path, Request, resource);
            }
            catch (Exception exception)
            {
                Log.Error("Error in GET", exception);
                return InternalServerError(exception);
            }
        }

        /// <summary>
        /// Returns a Http 200 Ok result
        /// </summary>
        /// <param name="allow"></param>
        /// <param name="davHeader"></param>
        /// <returns>HttpResonseMessage with OK Status</returns>
        public HttpResponseMessage Ok(IEnumerable<string> allow, string davHeader)
        {
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(string.Empty);
            response.Content.Headers.Add("Allow", allow);
            response.Headers.Add("DAV", davHeader);

            return response;
        }

        /// <summary>
        /// Creates and returns a multistatus response
        /// </summary>
        /// <param name="responses"></param>
        /// <returns></returns>
        public IHttpActionResult MultiStatus(List<Response> responses)
        {
            return new MultiStatusResponse(responses, Request);
        }


        /// <summary>
        /// Creates an returns a PrincipalSearchPropertySet response
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public IHttpActionResult PrincipalSearchPropertySet(params Tuple<string, string, string>[] properties)
        {
            return new PrincipalSearchPropertySetResponse(Request, properties);
        }

        /// <summary>
        /// returns an http created status
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage CalendarCreated()
        {
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        /// <summary>
        /// Load the requested Resource
        /// </summary>
        /// <param name="path">Path to resource</param>
        /// <returns>the requested Resource</returns>
        public abstract IDavResource LoadResource(string path);

        /// <summary>
        /// Load all children of the requested collection resource
        /// </summary>
        /// <param name="path">Path to resource</param>
        /// <returns>enumerable of all children of the requested collection resource</returns>
        public abstract IEnumerable<IDavResource> LoadCollectionResourceChildren(string path);

        /// <summary>
        /// Query resources
        /// </summary>
        /// <param name="path"></param>
        /// <param name="reportRequest"></param>
        /// <returns>enumerable of requested resources</returns>
        public abstract IEnumerable<IDavResource> QueryResources(string path, ReportRequest reportRequest);

        /// <summary>
        /// Multiget requested resources
        /// </summary>
        /// <param name="path"></param>
        /// <param name="reportRequest"></param>
        /// <returns>enumerable of requested resources</returns>
        public abstract IEnumerable<IDavResource> MultigetResources(string path, ReportRequest reportRequest);
    }
}
