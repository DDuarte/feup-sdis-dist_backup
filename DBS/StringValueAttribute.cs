using System;
using System.Linq;

namespace DBS
{
    /// <summary>
    /// Annotate enum with strings
    ///</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute : Attribute
    {
        // example usage:
        //   enum ShapeType
        //   {
        //      [StringValue("NONE")]
        //      None,
        //      [StringValue("SQUARESHAPE")]
        //      Square
        //   }
        //   ...
        //   var e = ShapeType.Square;
        //   var str = StringValueAttribute.Get(e); // str = "SQUARESHAPE"
        //   ...
        //   var str = "NONE";
        //   var e = StringValueAttribute.Get<ShapeType>(str); // e = ShapeType.None

        private readonly string _value;

        public StringValueAttribute(string value)
        {
            _value = value;
        }

        public static string Get<T>(T value)
        {
            string output = null;
            var type = value.GetType();
            var fi = type.GetField(value.ToString());
            var attrs = fi.GetCustomAttributes(typeof (StringValueAttribute), false) as StringValueAttribute[];
            if (attrs != null && attrs.Length > 0)
                output = attrs[0]._value;

            return output;
        }

        public static T Get<T>(string value)
        {
            // Iterate over all enum values and find the one with StringValue equal to the value we want 
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            return values.FirstOrDefault(v => value == Get(v));
        }
    }
}
