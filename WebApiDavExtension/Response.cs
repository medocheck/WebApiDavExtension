using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using WebApiDavExtension.WebDav;

namespace WebApiDavExtension
{
	public class Response
	{
	    protected Resource ResponseItem { get; }
        protected bool IsAllPropRequest { get; }
        protected IEnumerable<string> RequestedPropList { get; } = new List<string>();

        public Response(string href, Resource responseItem)
		{
			HRef = href;
            ResponseItem = responseItem;
            IsAllPropRequest = true;
        }

        public Response(string href, Resource responseItem, IEnumerable<string> requestedPropList)
        {
            HRef = href;
            ResponseItem = responseItem;
            RequestedPropList = requestedPropList;

            IsAllPropRequest = false;
        }

        public string HRef { get; set; }

		public XElement ResponseElement { get; private set; }

        protected XElement PropStatOkElement { get; private set; }
        protected XElement PropStatNotFoundElement { get; private set; }
        protected XElement PropStatForbiddenElement { get; private set; }

        protected XElement OkPropElement { get; private set; }
        protected XElement NotFoundPropElement { get; private set; }
        protected XElement ForbiddenPropElement { get; private set; }

        public XElement CreateXElement()
		{
			ResponseElement = new XElement(Namespaces.Dav + "response");

            if (!string.IsNullOrEmpty(HRef))
            {
                var hrefXElement = new XElement(Namespaces.Dav + "href", HRef);
                ResponseElement.Add(hrefXElement);
            }

            OkPropElement = new XElement(Namespaces.Dav + "prop");
            NotFoundPropElement = new XElement(Namespaces.Dav + "prop");
            ForbiddenPropElement = new XElement(Namespaces.Dav + "prop");

            PropStatOkElement = new XElement(
                Namespaces.Dav + "propstat", 
                OkPropElement, 
                new XElement(Namespaces.Dav + "status", $"HTTP/1.1 {(int)HttpStatusCode.OK} {HttpStatusCode.OK}"));

            PropStatNotFoundElement = new XElement(
                Namespaces.Dav + "propstat", 
                NotFoundPropElement,
                new XElement(Namespaces.Dav + "status", $"HTTP/1.1 {(int)HttpStatusCode.NotFound} {HttpStatusCode.NotFound}"));

            PropStatForbiddenElement = new XElement(
                Namespaces.Dav + "propstat",
                ForbiddenPropElement,
                new XElement(Namespaces.Dav + "status", $"HTTP/1.1 {(int)HttpStatusCode.Forbidden} {HttpStatusCode.Forbidden}"));

            CreateResponseElements();
		    UpdatePropStatElements();

            return ResponseElement;
		}

	    private void UpdatePropStatElements()
	    {
	        if (OkPropElement.HasElements && ResponseElement.Elements().All(e => e != PropStatOkElement))
	        {
	            ResponseElement.Add(PropStatOkElement);
	        }

	        if (NotFoundPropElement.HasElements && ResponseElement.Elements().All(e => e != PropStatNotFoundElement))
	        {
	            ResponseElement.Add(PropStatNotFoundElement);
	        }

	        if (ForbiddenPropElement.HasElements && ResponseElement.Elements().All(e => e != PropStatForbiddenElement))
	        {
	            ResponseElement.Add(PropStatForbiddenElement);
	        }
	    }

	    protected void CreateResponseElements()
        {
            var properties = GetProperties();

            if (IsAllPropRequest)
            {
                CreateAllPropResponse(properties);
            }
            else
            {
                CreateRequestedPropsResponse(properties);
            }
        }

        protected IEnumerable<Tuple<object, PropFindAttribute>> GetProperties()
	    {
	        var props = ResponseItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                prop => Attribute.IsDefined(prop, typeof(PropFindAttribute))).ToList();

	        return (from propertyInfo in props
                    let attribute = (PropFindAttribute) propertyInfo.GetCustomAttribute(typeof (PropFindAttribute))
                    let content = propertyInfo.GetValue(ResponseItem)
                    select new Tuple<object, PropFindAttribute>(content, attribute)).ToList();
	    }

        private void CreateAllPropResponse(IEnumerable<Tuple<object, PropFindAttribute>> properties)
        {
            foreach (var property in properties.Where(p => p.Item2.IsAllPropProperty))
            {
                AddOkProperty(property.Item1, property.Item2);
            }
        }

        private void CreateRequestedPropsResponse(IEnumerable<Tuple<object, PropFindAttribute>> properties)
        {
            foreach (string requestedProp in RequestedPropList)
            {
                if (!ResponseItem.HasProperty(requestedProp))
                {
                    AddNotFoundProperty(requestedProp);
                    continue;
                }

                var property = ResponseItem.GetProperty(requestedProp);
                AddOkProperty(property.Item1, property.Item2);
            }
        }

        public void AddNotFoundProperty(string name)
        {
            //XNamespace xNamespace = propNamespace;

            var emptyElement = new XElement(name);
            NotFoundPropElement.Add(emptyElement);
            UpdatePropStatElements();
        }

	    public void AddOkProperty(string name, string namespaceName)
	    {
            XNamespace xNamespace = namespaceName;

            var element = new XElement(xNamespace + name);
            OkPropElement.Add(element);
            UpdatePropStatElements();
        }

        public void AddOkProperty(object content, PropFindAttribute attribute)
        {
            if (attribute.IsList)
            {
                content = GetListItemsXml(content);
            }

            if (attribute.IsComplex)
            {
                content = GetXml(content);
            }

            XNamespace xNamespace = attribute.Namespace;
            var propContentElement = new XElement(xNamespace + attribute.Name, content);
            OkPropElement.Add(propContentElement);
        }

        protected IEnumerable<XElement> GetListItemsXml(object content)
        {
            var contentList = content as IEnumerable<object>;

            if (contentList != null)
            {
                return contentList.Select(GetXml).ToList();
            }

            return new List<XElement>();
        }

        protected XElement GetXml(object content)
        {
            if (content == null)
            {
                return new XElement("error", "No content submitted");
            }

            var element = content as XElement;

            if (element != null)
            {
                return element;
            }

            using (var writer = new StringWriter())
            {
                writer.NewLine = Environment.NewLine;
                var xmlSerializer = new XmlSerializer(content.GetType());

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                xmlSerializer.Serialize(writer, content, ns);

                XElement xElement = XElement.Parse(writer.ToString());

                return xElement;
            }
        }

        public void AddForbiddenProperty(string name, string namespaceName)
        {
            XNamespace xNamespace = namespaceName;

            var element = new XElement(xNamespace + name);
            ForbiddenPropElement.Add(element);
            UpdatePropStatElements();
        }
    }
}
