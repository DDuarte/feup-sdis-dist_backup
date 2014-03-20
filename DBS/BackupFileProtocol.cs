using System.IO;
using System.Linq;

namespace DBS
{
    /// <summary>
    /// Launches the backup file protocol
    /// </summary>
    public class BackupFileProtocol : IProtocol
    {
        private readonly string _fileName;
        private readonly FileEntry _fileEntry;

        private const int ChunkSize = 64000; // read the file in chunks of 64KB

        public BackupFileProtocol(string fileName, FileEntry fileEntry)
        {
            _fileName = fileName;
            _fileEntry = fileEntry;
        }

        public void Run()
        {
            //Task.Factory.StartNew(() =>
            //{
            using (var file = File.OpenRead(_fileName))
            {
                int bytesRead, chunkNo = 0;
                var fileSize = file.Length;
                var buffer = new byte[ChunkSize];
                while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    var data = buffer.Take(bytesRead).ToArray(); // slice the buffer with bytesRead
                    var bc = new BackupChunkSubprotocol(_fileEntry.FileId, chunkNo, _fileEntry.ReplicationDegree,
                        data);
                    bc.Run();
                    ++chunkNo;
                }

                if ((fileSize%ChunkSize) == 0) // last chunk with an empty body
                {
                    var bc = new BackupChunkSubprotocol(_fileEntry.FileId, chunkNo, _fileEntry.ReplicationDegree,
                        new byte[] {});
                    bc.Run();
                }
            }
            //});
        }
    }
}