﻿using System;
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

        public string FileName { get { return FileId + "_" + ChunkNo; } }
        public string FullFileName { get { return Path.Combine(Core.Instance.Config.BackupDirectory, FileName); } }

        public FileChunk(FileId fileId, int chunkNo)
        {
            FileId = fileId;
            ChunkNo = chunkNo;
        }

        public FileChunk(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException("filename");

            var tokens = filename.Split('_');

            if (tokens.Length != 2)
                throw new ArgumentException("Invalid filename", "filename");

            FileId = new FileId(tokens[0]);
            ChunkNo = int.Parse(tokens[1]);
        }

        public bool Exists()
        {
            return File.Exists(FullFileName);
        }

        public bool Delete()
        {
            if (!Exists())
                return false;

            try
            {
                File.Delete(FullFileName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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
                    var buffer = new byte[Core.Instance.Config.ChunkSize];
                    // try to read the maximum chunk size from the file
                    var bytesRead = fileData.Read(buffer, 0, buffer.Length);
                    _data = buffer.Take(bytesRead).ToArray(); // slice it
                    return _data;
                }
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("GetData()", ex);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true = file created, false = file exists, null = error</returns>
        public bool? SetData(byte[] data)
        {
            _data = data;

            if (Exists())
                return false;

            try
            {
                File.WriteAllBytes(FullFileName, _data);
                return true;
            }
            catch (Exception ex)
            {
                Core.Instance.Log.Error("FileChunk:SetData", ex);
                return null;
            }
        }

        public override string ToString()
        {
            return FileId.ToStringSmall() + "#" + ChunkNo;
        }
    }
}
