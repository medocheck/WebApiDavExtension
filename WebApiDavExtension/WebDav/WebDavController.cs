using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using log4net;
using WebApiDavExtension.CalDav;
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

        protected int GetIds(string path, out string principalId, out string calendarId)
        {
            principalId = string.Empty;
            calendarId = string.Empty;

            int result = 0;
            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (uriSegments.Length > 0)
            {
                principalId = uriSegments[0];
                result = 1;
            }

            if (uriSegments.Length > 1)
            {
                calendarId = uriSegments[1];
                result = 2;
            }

            return result;
        }

        protected int GetIds(string path, out string principalId, out string calendarId, out string eventId)
        {
            eventId = string.Empty;

            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int result = GetIds(path, out principalId, out calendarId);

            if (uriSegments.Length > 2)
            {
                eventId = uriSegments[2];

                if (eventId.EndsWith(".ics"))
                {
                    eventId = eventId.Substring(0, eventId.LastIndexOf(".ics", StringComparison.Ordinal));
                }

                result = 3;
            }

            return result;
        }

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
                
                string resourceHref = resource?.HRef ?? path;

                Response mainResponse = propFindRequest.CreateResponse(resourceHref, resource, false);
                responses.Add(mainResponse);

                if (propFindRequest.RequestDepth != RequestDepth.Zero && (resource is IDavCollectionResource || resource == null))
                {
                    IEnumerable<IDavResource> eventList = LoadCollectionResourceChildren(path);
                    responses.AddRange(
                        eventList.Select(
                            eventResource => propFindRequest.CreateResponse(eventResource.HRef, eventResource)));
                }

                return MultiStatus(responses);
            }
            catch (CalDavPathIsEmptyException exception)
            {
                Log.Warn("Path is empty in PROPFIND", exception);
                return new StatusCodeResult(HttpStatusCode.Forbidden, Request);
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
        /// Handles all REPORT requests without path
        /// </summary>
        /// <param name="reportRequest"></param>
        /// <returns></returns>
        [AcceptVerbs("REPORT")]
        public virtual IHttpActionResult Report(ReportRequest reportRequest)
        {
            try
            {
                Log.Debug("REPORT \t HRef: /");

                if (reportRequest.Type == ReportRequestType.PrincipalSearchPropertySet)
                {
                    return
                        PrincipalSearchPropertySet(
                            new Tuple<string, string, string>("DAV:", "displayname", "Display name"), new Tuple<string, string, string>("http://medocheck.com", "email-address", "Email address"));
                }

                return NotFound();
            }
            catch (Exception exception)
            {
                Log.Error("Error in REPORT", exception);
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
                LogReportRequest(reportRequest);

                if (reportRequest.Type == ReportRequestType.PrincipalSearchPropertySet)
                {
                    return PrincipalSearchPropertySetReport();
                }

                if (path == null)
                {
                    Log.Warn("Empty Href in REPORT");
                    return NotFound();
                }

                if (reportRequest.Type == ReportRequestType.SyncCollection)
                {
                    return SynCollectionReport(path, reportRequest);
                }

                IEnumerable<IDavResource> resources = reportRequest.Type == ReportRequestType.CalendarQuery
                    ? QueryResources(path, reportRequest)
                    : MultigetResources(path, reportRequest);

                List<Response> responses = new List<Response>();

                var davResources = resources as IDavResource[] ?? resources.ToArray();
                if (davResources.Any())
                {
                    responses.AddRange(davResources.Select(resource => reportRequest.CreateResponse(resource.HRef, resource)));
                }

                return MultiStatus(responses);
            }
            catch (Exception exception)
            {
                Log.Error("Error in REPORT", exception);
                return InternalServerError(exception);
            }
        }

        private IHttpActionResult SynCollectionReport(string path, ReportRequest reportRequest)
        {
            var syncToken = LoadCurrentSyncToken(path);
            var syncResources = LoadResourcesBySyncToken(path, reportRequest.SyncToken) ?? new List<IDavResource>();

            List<Response> syncResponses =
                syncResources.Select(resource => reportRequest.CreateResponse(resource.HRef, resource)).ToList();
            return MultiStatus(syncToken, syncResponses);
        }

        private IHttpActionResult PrincipalSearchPropertySetReport()
        {
            return
                PrincipalSearchPropertySet(
                    new Tuple<string, string, string>("DAV:", "displayname", "Display name"),
                    new Tuple<string, string, string>("http://medocheck.com", "email-address", "Email address"));
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
        /// Handle DELETE requests
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        public virtual IHttpActionResult Delete(string path)
        {
            try
            {
                if (RemoveResourse(path))
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception exception)
            {
                Log.Error("Error in DELETE", exception);
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
        /// Creates and returns a multistatus response
        /// </summary>
        /// <param name="responses"></param>
        /// <param name="syncToken"></param>
        /// <returns></returns>
        public IHttpActionResult MultiStatus(string syncToken, List<Response> responses)
        {
            return new MultiStatusResponse(syncToken, responses, Request);
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
        public IHttpActionResult Created()
        {
            var response = Request.CreateResponse(HttpStatusCode.Created);

            if (response.Content == null)
            {
                response.Content = new StringContent(string.Empty);
            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/calendar");
            return ResponseMessage(response);
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

        /// <summary>
        /// Get the current sync token
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract string LoadCurrentSyncToken(string path);

        /// <summary>
        /// Get all changed resources after the given sync token
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract IEnumerable<IDavResource> LoadResourcesBySyncToken(string path, string token); 

        /// <summary>
        /// Delete a resource
        /// </summary>
        /// <param name="path">Path to the respurce</param>
        /// <returns>true if resource was deleted; otherwise false</returns>
        public abstract bool RemoveResourse(string path);
    }
}
