using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBS.Protocols.Enhancements
{
    class EnhancedRestoreFileProtocol : IProtocol
    {
        private readonly FileEntry _fileEntry;
        public EnhancedRestoreFileProtocol(FileEntry fileEntry)
        {
            if (fileEntry == null)
                throw new ArgumentNullException("fileEntry");

            _fileEntry = fileEntry;
        }

        private void RestoreFile()
        {
            var fileName = Path.Combine(Core.Instance.RestoreDirectory, _fileEntry.FileName);
            using (var file = File.OpenWrite(fileName))
            {
                var chunkNo = 0;
                Message chunk;
                do
                {
                    var restoreChunkProtocol = new RestoreChunkSubprotocol(new FileChunk(_fileEntry.FileId, chunkNo));
                    Task.WaitAll(restoreChunkProtocol.Run());
                    chunk = restoreChunkProtocol.ChunkMessage;
                    if (chunk == null || chunk.Body == null || chunk.Body.Length == 0)
                        break;

                    file.Write(chunk.Body, 0, chunk.Body.Length);
                    ++chunkNo;
                } while (chunk.Body.Length == Core.Instance.ChunkSize);
            }
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    RestoreFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("RestoreFileProtocol:Run: could not restore file '{0}', ex: {1}",
                        _fileEntry.FileName, ex);
                }
            });
        }
    }
}
