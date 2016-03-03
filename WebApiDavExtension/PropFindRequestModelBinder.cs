using System.Xml.Linq;

namespace WebApiDavExtension
{
	public class PropFindRequestModelBinder : CalDavModelBinder<PropFindRequest>
	{
		internal override bool CreateModel(XDocument document)
		{
			return true;
		}
	}
}