using System;
using System.IO;
using System.Linq;

namespace DBS
{
    /// <summary>
    /// Class encapsulating a FileId and a chunk number
    /// </summary>
    public class FileChunk
    {
        public FileId FileId { get; private set; }
        public int ChunkNo { get; private set; }

        public string FileName { get { return FileId.ToString() + "_" + ChunkNo; } }
        public string FullFileName { get { return Path.Combine(Core.Instance.BackupDirectory, FileName); } }

        public FileChunk(FileId fileId, int chunkNo)
        {
            FileId = fileId;
            ChunkNo = chunkNo;
        }

        public bool Exists()
        {
            return File.Exists(FullFileName);
        }

        private byte[] _data;

        public byte[] GetData()
        {
            if (_data != null)
                return _data;

            if (!Exists())
                return null;

            try
            {
                using (var fileData = File.OpenRead(FullFileName))
                {
                    var buffer = new byte[Core.Instance.ChunkSize];
                    // try to read the maximum chunk size from the file
                    var bytesRead = fileData.Read(buffer, 0, buffer.Length);
                    _data = buffer.Take(bytesRead).ToArray(); // slice it
                }
            }
            catch (Exception)
            {
                // TODO: log message or something similar
                return null; 
            }

            return _data;
        }

        public override string ToString()
        {
            return FileId.ToStringSmall() + "#" + ChunkNo;
        }
    }
}
