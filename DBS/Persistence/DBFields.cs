using System;

namespace DBS.Persistence
{
    /// <summary>
    /// Class used in PersistentDictionary to serialize simple objects
    /// (like primitives, strings and classes with properties)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class DBFields<T>
    {
// ReSharper disable StaticFieldInGenericType
        private static readonly Lazy<string> _fields = new Lazy<string>(GetFields);
        private static readonly Lazy<string> _fieldsWithType = new Lazy<string>(GetFieldsWithType);
// ReSharper restore StaticFieldInGenericType

        public static string Fields { get { return _fields.Value; } }
        public static string FieldsWithType { get { return _fieldsWithType.Value; } }

        public static string Values(T obj) {  return GetValues(obj); }
        public static string FieldsAndValues(T obj) { return GetFieldsAndValues(obj); }

        private static string GetFields()
        {
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof (string))
                return type.Name;

            var props = typeof (T).GetProperties();
            var str = "";
            for (var i = 0; i < props.Length; i++)
            {
                str += props[i].Name;
                if (i != props.Length - 1)
                    str += ',';
            }
            return str;
        }

        private static string GetFieldsWithType()
        {
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof (string))
                return type.Name + ' ' + TypeToSQL(type);

            var props = typeof (T).GetProperties();
            var str = "";
            for (var i = 0; i < props.Length; i++)
            {
                str += props[i].Name + ' ' + TypeToSQL(props[i].PropertyType);
                if (i != props.Length - 1)
                    str += ',';
            }
            return str;
        }

        private static string GetValues(T obj)
        {
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof (string))
                return "'" + obj + "'";

            var props = type.GetProperties();
            var str = "";
            for (var i = 0; i < props.Length; i++)
            {
                str += "'" + props[i].GetValue(obj) + "'";
                if (i != props.Length - 1)
                    str += ',';
            }
            return str;
        }

        private static string GetFieldsAndValues(T obj)
        {
            var type = typeof(T);
            if (type.IsPrimitive || type == typeof (string))
                return type.Name + " = '" + obj + "'";

            var props = typeof (T).GetProperties();
            var str = "";
            for (var i = 0; i < props.Length; i++)
            {
                var f = props[i].Name;
                var v = props[i].GetValue(obj);

                str += f + " = '" + v + "'";
                if (i != props.Length - 1)
                    str += ',';
            }
            return str;
        }

        public static string TypeToSQL()
        {
            return TypeToSQL(typeof (T));
        }

        public static string TypeToSQL(Type t)
        {
            //var t = typeof (T);
            if (t == typeof (void))
                return "NULL";
            if (t == typeof (bool) || t == typeof (byte) ||
                t == typeof (char) || t == typeof (long) ||
                t == typeof (sbyte) || t == typeof (short) ||
                t == typeof (uint) || t == typeof (ulong) ||
                t == typeof (ushort) || t == typeof (int))
                return "INTEGER";
            if (t == typeof (decimal) || t == typeof (double) ||
                t == typeof (float))
                return "REAL";
            if (t == typeof (string))
                return "TEXT";
            return "BLOB"; // blob can be anything (sort of)
        }
    }
}
