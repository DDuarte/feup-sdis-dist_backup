using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peer
{
    public class FileEntry
    {
        public byte[] FileId { get; set; }

        public int ReplicationDegree { get; set; }
    }
}
