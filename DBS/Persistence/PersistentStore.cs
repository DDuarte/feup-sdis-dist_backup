using System;
using System.Collections;
using System.Collections.Generic;

namespace DBS.Persistence
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

        public int IncrementActualDegree(string fileName, int wantedDegree)
        {
            return IncrementActualDegree(fileName, 1, wantedDegree);
        }

        public int DecrementActualDegree(string fileName)
        {
            return IncrementActualDegree(fileName, -1, 1);
        }

        public void UpdateDegrees(string fileName, int actualDegree, int wantedDegree)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                t.ActualDegree = actualDegree;
                t.WantedDegree = wantedDegree;
            }
            else
                _dict[fileName] = new ReplicationDegrees {ActualDegree = actualDegree, WantedDegree = wantedDegree};
        }

        public bool TryGetDegrees(string fileName, out ReplicationDegrees rd)
        {
            return _dict.TryGetValue(fileName, out rd);
        }

        public bool RemoveDegrees(string fileName)
        {
            return _dict.Remove(fileName);
        }

        private int IncrementActualDegree(string fileName, int add, int wantedDegree)
        {
            ReplicationDegrees t;
            if (_dict.TryGetValue(fileName, out t))
            {
                t.ActualDegree += add;
                if (t.ActualDegree < 0)
                    t.ActualDegree = 0;
                return t.ActualDegree;
            }

            var actualDegree = Math.Max(add, 0);
            _dict[fileName] = new ReplicationDegrees { ActualDegree = actualDegree, WantedDegree = wantedDegree };
            return actualDegree;
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
