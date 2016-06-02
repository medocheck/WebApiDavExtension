using WebApiDavExtension.WebDav;

namespace WebApiDavExtension.CalDav
{
    public interface ICalendarResource : IDavResource
    {
        [PropFind("getcontenttype", Namespace = "DAV:")]
        string ContentType { get; }
    }
}
