using System;
using System.Collections.Generic;
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
                Core.Instance.Log.Error("GetDirectorySize", ex);
                return 0L;
            }
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new()
        {
            TValue val;

            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        public static IEnumerable<FileChunk> GetLocalFileChunks()
        {
            var fileList = Directory.GetFiles(Core.Instance.Config.BackupDirectory, "*_*");
            return fileList.Select(file => new FileChunk(Path.GetFileName(file)));
        }
    }
}
