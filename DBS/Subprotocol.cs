using System;
using System.IO;

namespace DBS
{
    interface ISubprotocol
    {
        void ProcessMessage(Message msg);
    }

    public class FileDeletionSubprotocol : ISubprotocol
    {
        public FileDeletionSubprotocol(string dir)
        {
            _directory = dir;
        }

        public void ProcessMessage(Message msg)
        {
            if (msg.MessageType != MessageType.Delete)
                return;

            if (!Directory.Exists(_directory))
                return;

            var fileIdStr = FileIdGenerator.FileIdToString(msg.FileId);

            var files = Directory.GetFiles(_directory, fileIdStr + "_*"); // ABCDEF01324_12 -- FileId_ChunkNo

            foreach (var file in files)
            {
                File.Delete(file);
                Console.WriteLine("Deleting file {0}", file);
                //PersistentStore.RemoveDegrees(file);
            }
        }

        private readonly string _directory;
    }

    //public class 
}
