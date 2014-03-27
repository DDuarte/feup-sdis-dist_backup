using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DBS.Protocols
{
    /// <summary>
    /// Launches the backup file protocol
    /// </summary>
    public class BackupFileProtocol : IProtocol
    {
        private readonly FileEntry _fileEntry;

        public BackupFileProtocol(FileEntry fileEntry)
        {
           _fileEntry = fileEntry;
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                using (var file = File.OpenRead(_fileEntry.OriginalFileName))
                {
                    int bytesRead, chunkNo = 0;
                    var buffer = new byte[Core.Instance.Config.ChunkSize];
                    while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        var data = buffer.Take(bytesRead).ToArray(); // slice the buffer with bytesRead
                        var bc = new BackupChunkSubprotocol(new FileChunk(_fileEntry.FileId, chunkNo), _fileEntry.ReplicationDegree,
                            data);
                        bc.Run().Wait();
                        ++chunkNo;
                    }

                    if ((file.Length % Core.Instance.Config.ChunkSize) == 0) // last chunk with an empty body
                    {
                        var bc = new BackupChunkSubprotocol(new FileChunk(_fileEntry.FileId, chunkNo), _fileEntry.ReplicationDegree,
                            new byte[] {});
                        bc.Run();
                    }
                }
            });
        }
    }
}