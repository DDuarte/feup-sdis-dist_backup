namespace Peer
{
    public class FileEntry
    {
        public byte[] FileId { get; set; }

        public int ReplicationDegree { get; set; }

        public int ActualReplicationDegree { get; set; }
    }
}
