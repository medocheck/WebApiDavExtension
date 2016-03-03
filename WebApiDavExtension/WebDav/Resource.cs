using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace WebApiDavExtension.WebDav
{
    public abstract class Resource
    {
        private IEnumerable<Tuple<object, PropFindAttribute>> _properties;

        public IEnumerable<Tuple<object, PropFindAttribute>> Properties => _properties ?? (_properties = GetProperties());

        public string HRef { get; set; }

        [PropFind("getetag", Namespace = "DAV:")]
        public object ETag { get; set; }

        public abstract MemoryStream GetOutputData();

        public bool HasProperty(string propertyName)
        {
            return Properties.Any(p => p.Item2.Name == propertyName);
        }

        public Tuple<object, PropFindAttribute> GetProperty(string popertyName)
        {
            return Properties.FirstOrDefault(p => p.Item2.Name == popertyName);
        }

        public object GetPropertyValue(string propertyName)
        {
            return Properties.FirstOrDefault(p => p.Item2.Name == propertyName);
        }

        public bool UpdateProperty(string propertyName, string stringValue)
        {
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                prop => Attribute.IsDefined(prop, typeof(PropFindAttribute))).ToList();

            var propertyInfo = (from pi in props
                let attribute = (PropFindAttribute) pi.GetCustomAttribute(typeof (PropFindAttribute))
                where attribute.Name == propertyName
                select pi).FirstOrDefault();

            if (propertyInfo == null)
            {
                return false;
            }

            var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
            var value = converter.ConvertFrom(stringValue);

            propertyInfo.SetValue(this, value);
            return true;
        }

        protected IEnumerable<Tuple<object, PropFindAttribute>> GetProperties()
        {
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                prop => Attribute.IsDefined(prop, typeof(PropFindAttribute))).ToList();

            return (from propertyInfo in props
                    let attribute = (PropFindAttribute)propertyInfo.GetCustomAttribute(typeof(PropFindAttribute))
                    let content = propertyInfo.GetValue(this)
                    select new Tuple<object, PropFindAttribute>(content, attribute)).ToList();
        }
    }
}
