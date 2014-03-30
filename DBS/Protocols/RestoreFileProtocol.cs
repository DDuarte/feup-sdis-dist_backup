using System;
using System.IO;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols
{
    class RestoreFileProtocol : IProtocol
    {
        private readonly FileEntry _fileEntry;

        public RestoreFileProtocol(FileEntry fileEntry)
        {
            if (fileEntry == null)
                throw new ArgumentNullException("fileEntry");

            _fileEntry = fileEntry;
        }

        private void RestoreFile()
        {
            var fileName = Path.Combine(Core.Instance.Config.RestoreDirectory, _fileEntry.FileName);
            using (var file = File.OpenWrite(fileName))
            {
                var chunkNo = 0;
                ChunkMessage chunk;
                bool success = true;
                do
                {
                    var restoreChunkProtocol = new RestoreChunkSubprotocol(new FileChunk(_fileEntry.GetFileId(), chunkNo));
                    restoreChunkProtocol.Run().Wait();
                    chunk = restoreChunkProtocol.Message;
                    if (chunk == null || chunk.Body == null || chunk.Body.Length == 0)
                    {
                        success = false;
                        break;
                    }

                    file.Write(chunk.Body, 0, chunk.Body.Length);
                    ++chunkNo;
                } while (chunk.Body.Length == Core.Instance.Config.ChunkSize);

                if (!success)
                {
                    Core.Instance.Log.ErrorFormat("RestoreFileProtocol:Run: could not restore file '{0}'",
                        _fileEntry.FileName);
                    File.Delete(fileName);
                }
                else
                    Core.Instance.Log.InfoFormat("RestoreFileProtocol:Run: restore file successful '{0}'",
                        _fileEntry.FileName);
            }
        }

        public Task Run()
        {
            Core.Instance.Log.InfoFormat("Starting RestoreFileProtocol: {0}", _fileEntry);
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    RestoreFile();
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.ErrorFormat("RestoreFileProtocol:Run: could not restore file '{0}', ex: {1}",
                        _fileEntry.FileName, ex);
                    try
                    {
                        File.Delete(Path.Combine(Core.Instance.Config.RestoreDirectory, _fileEntry.FileName));
                    }
// ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // swallow it up 
                    }
                }
            });
        }
    }
}
