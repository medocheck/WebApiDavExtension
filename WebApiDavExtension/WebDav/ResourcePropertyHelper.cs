using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace WebApiDavExtension.WebDav
{
    internal static class ResourcePropertyHelper
    {
        public static bool HasProperty(IDavResource resource, string propertyName)
        {
            if (resource == null)
            {
                return false;
            }

            return GetProperties(resource).Any(p => p.Item2.Name == propertyName);
        }

        public static Tuple<object, PropFindAttribute> GetProperty(IDavResource resource, string popertyName)
        {
            if (resource == null)
            {
                return null;
            }

            return GetProperties(resource).FirstOrDefault(p => p.Item2.Name == popertyName);
        }

        public static object GetPropertyValue(IDavResource resource, string propertyName)
        {
            return GetProperties(resource).FirstOrDefault(p => p.Item2.Name == propertyName);
        }

        public static bool UpdateProperty(IDavResource resource, string propertyName, string stringValue)
        {
            var props = resource.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                prop => Attribute.IsDefined(prop, typeof(PropFindAttribute))).ToList();

            var propertyInfo = (from pi in props
                                let attribute = (PropFindAttribute)pi.GetCustomAttribute(typeof(PropFindAttribute))
                                where attribute.Name == propertyName
                                select pi).FirstOrDefault();

            if (propertyInfo == null)
            {
                return false;
            }

            var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
            var value = converter.ConvertFrom(stringValue);

            propertyInfo.SetValue(resource, value);
            return true;
        }

        private static IEnumerable<Tuple<object, PropFindAttribute>> GetProperties(IDavResource resource)
        {
            var props = resource.GetType().GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance).ToList();

            props = props.Where(
                prop => Attribute.IsDefined(prop, typeof(PropFindAttribute))).ToList();

            return (from propertyInfo in props
                    let attribute = (PropFindAttribute)propertyInfo.GetCustomAttribute(typeof(PropFindAttribute))
                    let content = propertyInfo.GetValue(resource)
                    select new Tuple<object, PropFindAttribute>(content, attribute)).ToList();
        }
    }
}
