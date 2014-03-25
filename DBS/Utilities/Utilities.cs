using System;
using System.IO;
using System.Linq;

namespace DBS.Utilities
{
    public static class Utilities
    {
        public static string FormatWith(this string format, params object[] args)
        {
            return String.Format(format, args);
        }

        public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
        {
            return String.Format(provider, format, args);
        }

        public static DirectoryInfo CreateDirectoryIfNotExists(string path)
        {
            return Directory.Exists(path) ? new DirectoryInfo(path) : Directory.CreateDirectory(path);
        }

        public static long GetDirectorySize(string p)
        {
            try
            {
                return Directory.GetFiles(p).Select(name => new FileInfo(name)).Select(info => info.Length).Sum();
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetDirectorySize: {0}", ex);
                return 0L;
            }
        }
    }
}
