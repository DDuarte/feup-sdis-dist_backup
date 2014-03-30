using System;
using System.Collections.Generic;
using DBS.Protocols;
using DBS.Protocols.Enhancements;

namespace DBS
{
    public interface ICommand
    {
        void Execute();
    }

    public class CommandSwitch
    {
        private readonly List<ICommand> _commands = new List<ICommand>();

        public void Execute(ICommand cmd)
        {
            _commands.Add(cmd);
            cmd.Execute();
        }
    }

    public class BackupFileCommand : ICommand
    {
        private readonly FileEntry _fileEntry;

        public BackupFileCommand(FileEntry fileEntry)
        {
            if (fileEntry == null)
                throw new ArgumentNullException("fileEntry");

            _fileEntry = fileEntry;
        }

        public void Execute()
        {
            new BackupFileProtocol(_fileEntry).Run();
        }
    }

    public class RestoreFileCommand : ICommand
    {
        private readonly FileEntry _fileEntry;
        private readonly bool _enhanced;

        public RestoreFileCommand(FileEntry fileEntry, bool enhanced = false)
        {
            if (fileEntry == null)
                throw new ArgumentNullException("fileEntry");

            _fileEntry = fileEntry;
            _enhanced = enhanced;
        }

        public void Execute()
        {
            if (_enhanced)
                new EnhancedRestoreFileProtocol(_fileEntry).Run();
            else
                new RestoreFileProtocol(_fileEntry).Run();
        }
    }

    public class DeleteFileCommand : ICommand
    {
        private readonly FileEntry _fileEntry;

        public DeleteFileCommand(FileEntry fileEntry)
        {
            if (fileEntry == null)
                throw new ArgumentNullException("fileEntry");

            _fileEntry = fileEntry;
        }

        public void Execute()
        {
            new DeleteFileProtocol(_fileEntry).Run();
        }
    }

    public class SpaceReclaimingCommand : ICommand
    {
        public void Execute()
        {
            new SpaceReclaimingProtocol(true).Run();
        }
    }
}
