using System.IO;

namespace WebApiDavExtension.WebDav
{
    public interface IDavResource
    {
        string HRef { get; set; }

        [PropFind("getetag", Namespace = "DAV:")]
        object ETag { get; set; }

        MemoryStream GetOutputData();
    }
}
