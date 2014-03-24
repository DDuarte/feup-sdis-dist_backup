using System.Collections.Generic;

namespace DBS
{
    public class FileEntry
    {
        public FileId FileId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public int ReplicationDegree { get; set; }

        /// <summary>
        /// Two FileEntry are equal if they have the same FileName
        /// </summary>
        public class Comparer : IEqualityComparer<FileEntry>
        {
            public bool Equals(FileEntry x, FileEntry y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                if (x.FileName == null && y.FileName == null)
                    return true;
                if (x.FileName == null || y.FileName == null)
                    return false;

                return x.FileName == y.FileName;
            }

            public int GetHashCode(FileEntry obj)
            {
                if (obj == null || obj.FileName == null)
                    return 0;
                return obj.FileName.GetHashCode();
            }
        }
    }
}
