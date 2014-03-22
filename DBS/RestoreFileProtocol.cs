using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBS
{
    class RestoreFileProtocol : IProtocol
    {
        private readonly string _fileName;
        private readonly FileEntry _fileInfo;

        public RestoreFileProtocol(string fileName, FileEntry fileInfo)
        {
            _fileName = fileName;
            _fileInfo = fileInfo;
        }

        public void Run()
        {
            Task.Factory.StartNew(() =>
            {
                using (var file = File.OpenWrite(_fileName))
                {
                    int chunkNo = 0;
                    Message chunk;
                    do
                    {
                        var restoreChunkProtocol = new RestoreChunkSubprotocol(_fileInfo.FileId, chunkNo);
                        restoreChunkProtocol.Run();
                        chunk = restoreChunkProtocol.Chunk;
                        file.Write(chunk.Body, 0, chunk.Body.Length);
                        ++chunkNo;
                    } while (chunk.Body.Length == 64000);
                }
            });
        }
    }
}

