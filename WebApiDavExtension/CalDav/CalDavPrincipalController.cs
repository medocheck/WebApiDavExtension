using System;
using System.Collections.Generic;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
    public abstract class CalDavPrincipalController : WebDavController
    {
        public override IDavResource LoadResource(string path)
        {
            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (uriSegments.Length == 0)
            {
                throw new Exception("No Principal Id submitted");
            }

            return LoadPrincipal(uriSegments[0]);
        }

        public override IEnumerable<IDavResource> LoadCollectionResourceChildren(string path)
        {
            return new[] {LoadResource(path) };
        }

        public override IEnumerable<IDavResource> QueryResources(string path, ReportRequest reportRequest)
        {
            return new [] { LoadResource(path) };
        }

        public override IEnumerable<IDavResource> MultigetResources(string path, ReportRequest reportRequest)
        {
            return new[] { LoadResource(path) };
        }

        public abstract IDavPrincipal LoadPrincipal(string principalId);
    }
}
