using System;

namespace WebApiDavExtension
{
	public class PropFindAttribute : Attribute
	{
		public PropFindAttribute(string name)
		{
			Name = name;
			IsAllPropProperty = false;
		}

		public PropFindAttribute(string name, bool isAllPropProperty)
		{
			Name = name;
			IsAllPropProperty = isAllPropProperty;
		}

		public string Name { get; }
		public string Namespace { get; set; } = "";
		public bool IsAllPropProperty { get; set; }
		public bool IsList { get; set; }
		public bool IsComplex { get; set; }
	}
}
