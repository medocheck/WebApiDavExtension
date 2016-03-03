using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace WebApiDavExtension
{
    public class CalDavValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new CalDavValueProvider(actionContext);
        }
    }
}