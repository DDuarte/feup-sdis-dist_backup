using System;
using System.Collections;
using System.Collections.Generic;
using JsonConfig;

namespace Peer
{
    public class ReplicationDegrees
    {
        public int ActualDegree { get; set; }
        public int WantedDegree { get; set; }
    }

    public class PersistentStore : IEnumerable<KeyValuePair<string, ReplicationDegrees>>, IDisposable
    {
        private readonly PersistentDictionary<string, ReplicationDegrees> _dict =
            new PersistentDictionary<string, ReplicationDegrees>("store", "repdegrees");

        public bool ContainsFile(string fileName)
        {
            return _dict.ContainsKey(fileName);
        }

        public void IncrementActualDegree(string fileName, int wantedDegree)
        {
            IncrementActualDegree(fileName, 1, wantedDegree);
        }

        public void DecrementActualDegree(string fileName, int wantedDegree)
        {
            IncrementActualDegree(fileName, -1, wantedDegree);
        }

        public void UpdateDegrees(string fileName, int actualDegree, int wantedDegree)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                _dict[fileName].ActualDegree = t.ActualDegree;
                _dict[fileName].WantedDegree = t.WantedDegree;
            }
            else
                _dict[fileName] = new ReplicationDegrees {ActualDegree = actualDegree, WantedDegree = wantedDegree};
        }

        public bool TryGetDegrees(string fileName, out ReplicationDegrees rd)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                rd = t;
                return true;
            }
            rd = null;
            return false;
        }

        public bool RemoveDegrees(string fileName)
        {
            return _dict.Remove(fileName);
        }

        private void IncrementActualDegree(string fileName, int add, int wantedDegree)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                var t2 = new ReplicationDegrees {ActualDegree = t.ActualDegree + add, WantedDegree = t.WantedDegree};
                _dict[fileName] = t2;
            }
            else
            {
                _dict[fileName] = new ReplicationDegrees { ActualDegree = add < 0 ? 0 : 1, WantedDegree = wantedDegree };
            }
        }

        public IEnumerator<KeyValuePair<string, ReplicationDegrees>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dict).GetEnumerator();
        }

        protected virtual void Dispose(bool d)
        {
            if (d)
                _dict.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
