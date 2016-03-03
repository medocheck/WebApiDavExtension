using System.Web.Http;
using System.Web.Http.ValueProviders;
using WebApiDavExtension.Authentication;

namespace WebApiDavExtension.example
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
            // Web API configuration and services
            config.Filters.Add(new BasicAuthorizationFilterAttribute(false));
            config.Filters.Add(new DigestAuthorizationFilterAttribute());

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "WebDavApi",
                routeTemplate: "api/Calendar/{*path}",
                defaults: new { controller = "Calendar" }
            );

            config.Routes.MapHttpRoute(
                name: "PrincipalRoute",
                routeTemplate: "api/Principals/{*path}",
                defaults: new { controller = "Principal" }
            );

			config.Services.Add(typeof(ValueProviderFactory), new CalDavValueProviderFactory());
            log4net.Config.XmlConfigurator.Configure();
        }
	}
}
