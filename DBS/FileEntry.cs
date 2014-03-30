namespace DBS
{
    public class FileEntry
    {
        private FileId _fileId;
        private string _originalFileName;

        public FileId GetFileId() {  return _fileId; } // because we are storing this class in db it can't be property
        public string FileName { get; set; }

        public string OriginalFileName
        {
            get { return _originalFileName; }
            set
            {
                _originalFileName = value;
                _fileId = FileId.FromFile(_originalFileName);
            }
        }

        public int ReplicationDegree { get; set; }

        public override string ToString()
        {
            return string.Format("FileId: {0}, FileName: {1}, OriginalFileName: {2}, ReplicationDegree: {3}",
                GetFileId().ToStringSmall(), FileName, OriginalFileName, ReplicationDegree);
        }
    }
}
