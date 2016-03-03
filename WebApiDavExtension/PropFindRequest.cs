using System.Web.Http.ModelBinding;

namespace WebApiDavExtension
{
	[ModelBinder(typeof(PropFindRequestModelBinder))]
	public class PropFindRequest : Request
	{
		
	}
}