using System;
using System.IO;
using System.Reactive.Linq;

namespace DBS
{
    /// <summary>
    /// Listens to DELETE messages on MC
    /// </summary>
    public class FileDeletionService : IService
    {
        public void Start()
        {
            Core.Instance.MCChannel.Received
                .Where(message => message.MessageType == MessageType.Delete)
                .Subscribe(this);
        }

        public void OnNext(Message msg)
        {
            var dir = Core.Instance.BackupDirectory;
            if (!Directory.Exists(dir))
                return;

            var fileIdStr = msg.FileId.ToString();

            // Possible idea: store all chunks of the same file in a directory (name = FileId) and do Directory.Delete(dir, true);
            var files = Directory.GetFiles(dir, fileIdStr + "_*"); // ABCDEF01324_12 -- FileId_ChunkNo
            foreach (var file in files)
            {
                File.Delete(file);
                Console.WriteLine("FileDeletionService: Deleting file {0}", file);
                Core.Instance.Store.RemoveDegrees(file);
            }
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("FileDeletionService:OnError: {0}", error);
        }

        public void OnCompleted()
        {
            Console.WriteLine("FileDeletionService:OnCompleted");
        }

        public void Stop()
        {
            Console.WriteLine("FileDeletionService:Stop");
        }
    }
}