using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBS.Messages
{
    class PutChunkMessage : Message
    {
        private FileChunk _fileChunk;
        private int _replicationDeg;
        private byte[] _body;

        public PutChunkMessage(int versionM, int versionN, FileChunk fileChunk, int replicationDeg, byte[] body)
            : base(versionM, versionN, MessageType.PutChunk)
        {
            _fileChunk = fileChunk;
            _replicationDeg = replicationDeg;
            _body = body;
        }

        public override byte[] Serialize()
        {
            
        }

        public override Message Deserialize()
        {
            
        }
    }
}
