using System.IO;
using System.Threading.Tasks;

namespace DBS
{
    class RestoreFileProtocol : IProtocol
    {
        private readonly string _fileName;
        private readonly FileEntry _fileEntry;

        public RestoreFileProtocol(string fileName, FileEntry fileEntry)
        {
            _fileName = fileName;
            _fileEntry = fileEntry;
        }

        public void Run()
        {
            Task.Factory.StartNew(() =>
            {
                using (var file = File.OpenWrite(_fileName))
                {
                    var chunkNo = 0;
                    Message chunk;
                    do
                    {
                        var restoreChunkProtocol = new RestoreChunkSubprotocol(new FileChunk(_fileEntry.FileId, chunkNo));
                        restoreChunkProtocol.Run();
                        chunk = restoreChunkProtocol.ChunkMessage;
                        if (chunk == null || chunk.Body == null || chunk.Body.Length == 0)
                            break;

                        file.Write(chunk.Body, 0, chunk.Body.Length);
                        ++chunkNo;
                    } while (chunk.Body.Length == 64000);
                }
            });
        }
    }
}
