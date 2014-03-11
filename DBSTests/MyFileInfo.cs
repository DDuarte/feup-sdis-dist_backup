using System;
using System.IO;
using System.Security.AccessControl;
using SystemWrapper;
using SystemWrapper.IO;
using SystemWrapper.Security.AccessControl;

namespace DBSTests
{
    internal class MyFileInfo : IFileInfoWrap
    {
        public MyFileInfo(IDateTimeWrap creationTime, IDateTimeWrap lastWriteTime, string name, byte[] data)
        {
            CreationTime = creationTime;
            LastWriteTime = lastWriteTime;
            Name = name;
            _data = data;
        }

        private readonly byte[] _data;

        public void Initialize(FileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string fileName)
        {
            throw new NotImplementedException();
        }

        public IStreamWriterWrap AppendText()
        {
            throw new NotImplementedException();
        }

        public IFileInfoWrap CopyTo(string destFileName)
        {
            throw new NotImplementedException();
        }

        public IFileInfoWrap CopyTo(string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public IFileStreamWrap Create()
        {
            throw new NotImplementedException();
        }

        public IStreamWriterWrap CreateText()
        {
            throw new NotImplementedException();
        }

        public void Decrypt()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Encrypt()
        {
            throw new NotImplementedException();
        }

        public IFileSecurityWrap GetAccessControl()
        {
            throw new NotImplementedException();
        }

        public IFileSecurityWrap GetAccessControl(AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public void MoveTo(string destFileName)
        {
            throw new NotImplementedException();
        }

        public IFileStreamWrap Open(FileMode mode)
        {
            throw new NotImplementedException();
        }

        public IFileStreamWrap Open(FileMode mode, FileAccess access)
        {
            return new MyFileStream(_data);
        }

        public IFileStreamWrap Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public IFileStreamWrap OpenRead()
        {
            throw new NotImplementedException();
        }

        public IStreamReaderWrap OpenText()
        {
            throw new NotImplementedException();
        }

        public IFileStreamWrap OpenWrite()
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public IFileInfoWrap Replace(string destinationFileName, string destinationBackupFileName)
        {
            throw new NotImplementedException();
        }

        public IFileInfoWrap Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        public void SetAccessControl(IFileSecurityWrap fileSecurity)
        {
            throw new NotImplementedException();
        }

        public FileAttributes Attributes { get; set; }
        public IDateTimeWrap CreationTime { get; set; }
        public IDateTimeWrap CreationTimeUtc { get; set; }
        public IDirectoryInfoWrap Directory { get; private set; }
        public string DirectoryName { get; private set; }
        public bool Exists { get; private set; }
        public string Extension { get; private set; }
        public FileInfo FileInfoInstance { get; private set; }
        public string FullName { get; private set; }
        public bool IsReadOnly { get; set; }
        public IDateTimeWrap LastAccessTime { get; set; }
        public IDateTimeWrap LastAccessTimeUtc { get; set; }
        public IDateTimeWrap LastWriteTime { get; set; }
        public IDateTimeWrap LastWriteTimeUtc { get; set; }
        public long Length { get; private set; }
        public string Name { get; private set; }
    }
}