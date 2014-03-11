using System;
using System.IO;
using System.Security.AccessControl;
using SystemWrapper.IO;
using SystemWrapper.Microsoft.Win32.SafeHandles;
using SystemWrapper.Security.AccessControl;

namespace DBSTests
{
    internal class MyFileStream : IFileStreamWrap
    {
        public MyFileStream(byte[] data)
        {
            CanRead = true;
            Length = data.Length;
            _data = data;
            StreamInstance = new MemoryStream(_data);
        }

        private readonly byte[] _data;

        public void Dispose()
        {
        }

        public IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
        }

        public int EndRead(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            Array.Copy(_data, 0, buffer, offset, count);
            return Math.Min(_data.Length, count);
        }

        public int ReadByte()
        {
            return 1;
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public IStreamWrap Synchronized(IStreamWrap stream)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }

        public bool CanRead { get; private set; }
        public bool CanSeek { get; private set; }
        public bool CanTimeout { get; private set; }
        public bool CanWrite { get; private set; }
        public long Length { get; private set; }
        public long Position { get; set; }
        public int ReadTimeout { get; set; }
        public Stream StreamInstance { get; private set; }
        public int WriteTimeout { get; set; }
        public void Initialize(Stream stream)
        {
            throw new NotImplementedException();
        }

        public void Initialize(FileStream fileStream)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ISafeFileHandleWrap handle, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ISafeFileHandleWrap handle, FileAccess access, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ISafeFileHandleWrap handle, FileAccess access, int bufferSize, bool isAsync)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize,
            FileOptions options)
        {
            throw new NotImplementedException();
        }

        public void Initialize(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize,
            FileOptions options, FileSecurity fileSecurity)
        {
            throw new NotImplementedException();
        }

        public IFileSecurityWrap GetAccessControl()
        {
            throw new NotImplementedException();
        }

        public void Lock(long position, long length)
        {
            throw new NotImplementedException();
        }

        public void SetAccessControl(IFileSecurityWrap fileSecurity)
        {
            throw new NotImplementedException();
        }

        public void Unlock(long position, long length)
        {
            throw new NotImplementedException();
        }

        public FileStream FileStreamInstance { get; private set; }
        public bool IsAsync { get; private set; }
        public string Name { get; private set; }
        public ISafeFileHandleWrap SafeFileHandle { get; private set; }
    }
}