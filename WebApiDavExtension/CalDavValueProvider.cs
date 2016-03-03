using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using System.Xml;
using System.Xml.Linq;
using log4net;


namespace WebApiDavExtension
{
    internal class CalDavValueProvider : IValueProvider
    {
        protected static ILog Log { get; } =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _rawValue;
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public CalDavValueProvider(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            XDocument document;
            _rawValue = GetRequestBody(actionContext.Request);

            if (_rawValue.StartsWith("<?xml") && TryParseXml(_rawValue, out document))
            {
                _data["content"] = document;
            }
            else
            {
                _data["content"] = _rawValue;
            }

            _data["depth"] = actionContext.Request.Headers.Contains("Depth")
                ? actionContext.Request.Headers.First(h => h.Key == "Depth").Value.First()
                : "0";
        }

        private string GetRequestBody(HttpRequestMessage requestMessage)
        {
            try
            {
                if (requestMessage.Content.IsMimeMultipartContent())
                {
                    var res = requestMessage.Content.ReadAsMultipartAsync().Result;
                    return res.ToString();
                }

                var reqStream = requestMessage.Content.ReadAsStreamAsync();

                using (MemoryStream ms = new MemoryStream())
                {
                    reqStream.Result.CopyTo(ms);
                    return System.Text.Encoding.Default.GetString(ms.ToArray());
                }
            }
            catch (Exception exception)
            {
                Log.Error("Could not get request body", exception);
                return string.Empty;
            }
        }

        private static bool TryParseXml(string value, out XDocument document)
        {
            try
            {
                document = XDocument.Parse(value);
                return true;
            }
            catch (XmlException)
            {
                document = null;
                return false;
            }
        }

        public bool ContainsPrefix(string prefix)
        {
            return _rawValue.Contains(prefix);
        }

        public ValueProviderResult GetValue(string key)
        {
            return new ValueProviderResult(_data[key], _rawValue, CultureInfo.InvariantCulture);
        }
    }
}
