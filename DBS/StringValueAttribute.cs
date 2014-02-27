using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBS
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute : Attribute
    {
        public StringValueAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public static string GetStringValue(Enum value)
        {
            string output = null;
            var type = value.GetType();
            var fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
            if (attrs != null && attrs.Length > 0)
                output = attrs[0].Value;

            return output;
        }
    }
}
