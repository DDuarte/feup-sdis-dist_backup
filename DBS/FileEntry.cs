using System.Collections.Generic;

namespace DBS
{
    public class FileEntry
    {
        public FileId FileId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public int ReplicationDegree { get; set; }

        public override string ToString()
        {
            return string.Format("FileId: {0}, FileName: {1}, OriginalFileName: {2}, ReplicationDegree: {3}",
                FileId.ToStringSmall(), FileName, OriginalFileName, ReplicationDegree);
        }

        /// <summary>
        /// Two FileEntry are equal if they have the same FileId
        /// </summary>
        public class Comparer : IEqualityComparer<FileEntry>
        {
            public bool Equals(FileEntry x, FileEntry y)
            {
                if (x == null && y == null)
                    return true;
                if (x == null || y == null)
                    return false;
                if (x.FileId == null && y.FileId == null)
                    return true;
                if (x.FileId == null || y.FileId == null)
                    return false;

                return x.FileId == y.FileId;
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
