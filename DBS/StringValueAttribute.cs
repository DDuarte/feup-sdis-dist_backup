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
        private readonly string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public static string Get(Enum value)
        {
            string output = null;
            var type = value.GetType();
            var fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
            if (attrs != null && attrs.Length > 0)
                output = attrs[0]._value;

            return output;
        }
    }
}
