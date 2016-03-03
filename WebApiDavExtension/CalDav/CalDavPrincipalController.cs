using System;
using System.Collections.Generic;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
    public abstract class CalDavPrincipalController : WebDavController
    {
        public override Resource LoadResource(string path)
        {
            string[] uriSegments = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (uriSegments.Length == 0)
            {
                throw new Exception("No Principal Id submitted");
            }

            return LoadPrincipal(uriSegments[0]);
        }

        public override IEnumerable<Resource> LoadCollectionResourceChildren(string path)
        {
            return new[] {LoadResource(path) };
        }

        public override IEnumerable<Resource> QueryResources(string path, ReportRequest reportRequest)
        {
            return new [] { LoadResource(path) };
        }

        public override IEnumerable<Resource> MultigetResources(string path, ReportRequest reportRequest)
        {
            return new[] { LoadResource(path) };
        }

        public abstract Principal LoadPrincipal(string principalId);
    }
}
