using System;
using System.IO;
using System.Threading.Tasks;
using DBS.Messages;

namespace DBS.Protocols.Enhancements
{
    class EnhancedRestoreFileProtocol : IProtocol
    {
        private readonly FileEntry _fileEntry;
        private const int MaxEntries = 5;

        public EnhancedRestoreFileProtocol(FileEntry fileEntry)
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
                ChunkMessage chunk = null;
                bool success;
                do
                {
                    success = true;
                    for (int retryCount = 0; retryCount < MaxEntries; retryCount++)
                    {
                        var restoreChunkProtocol =
                            new EnhancedRestoreChunkProtocol(new FileChunk(_fileEntry.GetFileId(), chunkNo));
                        restoreChunkProtocol.Run().Wait();
                        chunk = restoreChunkProtocol.Message;
                        if (chunk != null && chunk.Body != null && chunk.Body.Length > 0)
                        {
                            file.Write(chunk.Body, 0, chunk.Body.Length);
                            break;
                        }

                        if (retryCount == MaxEntries - 1)
                            success = false;
                    }

                    if (!success)
                        break;

                    ++chunkNo;
                } while (chunk != null && chunk.Body != null && chunk.Body.Length == Core.Instance.Config.ChunkSize);

                if (success)
                    Core.Instance.Log.InfoFormat("EnhancedRestoreFileProtocol: file '{0}' was sucessfuly restored",
                        _fileEntry.FileName);
                else
                {
                    Core.Instance.Log.ErrorFormat("EnhancedRestoreFileProtocol: file '{0}' restore failed",
                        _fileEntry.FileName);
                    try
                    {
                        File.Delete(fileName);
                    }
// ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                        // swallow exception
                    }
                }
            }
        }

        public Task Run()
        {
            Core.Instance.Log.InfoFormat("Starting EnhancedRestoreFileProtocol: {0}", _fileEntry);
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
                }
            });
        }
    }
}
