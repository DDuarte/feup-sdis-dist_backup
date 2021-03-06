using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using DBS.Messages;

namespace DBS.Protocols
{
    /// <summary>
    /// Listens to DELETE messages on MC
    /// </summary>
    public class DeleteFileService : IServiceObserver<DeleteMessage>
    {
        public void Start()
        {
            Core.Instance.Log.Info("Starting DeleteFileService");
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Delete)
                .Cast<DeleteMessage>()
                .Subscribe(this);
        }

        public void OnNext(DeleteMessage msg)
        {
            var dir = Core.Instance.Config.BackupDirectory;
            if (!Directory.Exists(dir))
                return;

            var toRemove = Core.Instance.BackupFiles
                .Where(pair => pair.Value.GetFileId() == msg.FileId)
                .Select(pair => pair.Key);
            foreach (var fileName in toRemove)
                Core.Instance.BackupFiles.Remove(fileName);
            Core.Instance.ChunkPeers.RemoveAllChunkPeer(msg.FileId);

            var fileIdStr = msg.FileId.ToString();

            // Possible idea: store all chunks of the same file in a directory (name = FileId) and do Directory.Delete(dir, true);
            var files = Directory.GetFiles(dir, fileIdStr + "_*"); // ABCDEF01324_12 -- FileId_ChunkNo
            foreach (var file in files)
            {
                File.Delete(file);
                Core.Instance.Log.InfoFormat("DeleteFileService: Deleting file {0}", file);
            }
        }

        public void OnError(Exception error)
        {
            Core.Instance.Log.Error("DeleteFileService:OnError", error);
        }

        public void OnCompleted()
        {
            Core.Instance.Log.Info("DeleteFileService:OnCompleted");
        }

        public void Stop()
        {
            Core.Instance.Log.Info("DeleteFileService:Stop");
        }
    }
}