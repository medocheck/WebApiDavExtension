using System.Collections.Generic;

namespace WebApiDavExtension.WebDav
{
    public interface IDavCollectionResource : IDavResource
    {
        List<IDavResource> Resources { get; }
    }
}
