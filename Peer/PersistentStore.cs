using System;
using JsonConfig;
using Microsoft.Isam.Esent.Collections.Generic;

namespace Peer
{
    [Serializable]
    public struct ReplicationDegrees
    {
        public ReplicationDegrees(int a, int w)
        {
            ActualDegree = a;
            WantedDegree = w;
        }

        public readonly int ActualDegree;
        public readonly int WantedDegree;
    }

    public static class PersistentStore
    {
        // TODO: Add enumerator and change to private
        public static readonly PersistentDictionary<string, ReplicationDegrees> Dict = new PersistentDictionary<string, ReplicationDegrees>(Config.Global.StoreDir);

        public static void IncrementActualDegree(string fileName, int wantedDegree)
        {
            IncrementActualDegree(fileName, 1, wantedDegree);
        }

        public static void DecrementActualDegree(string fileName, int wantedDegree)
        {
            IncrementActualDegree(fileName, -1, wantedDegree);
        }

        public static void UpdateDegrees(string fileName, int actualDegree, int wantedDegree)
        {
            ReplicationDegrees t;
            if (Dict.TryGetValue(fileName, out t))
            {
                var t2 = new ReplicationDegrees(t.ActualDegree, t.WantedDegree);
                Dict[fileName] = t2;
            }
            else
            {
                Dict[fileName] = new ReplicationDegrees(actualDegree, wantedDegree);
            }
        }

        public static bool TryGetDegrees(string fileName, out ReplicationDegrees rd)
        {
            ReplicationDegrees t;
            if (Dict.TryGetValue(fileName, out t))
            {
                rd = t;
                return true;
            }
            else
            {
                rd = new ReplicationDegrees(0, 0);
                return false;
            }
        }

        public static bool RemoveDegrees(string fileName)
        {
            return Dict.Remove(fileName);
        }

        private static void IncrementActualDegree(string fileName, int actualDegree, int wantedDegree)
        {
            ReplicationDegrees t;
            if (Dict.TryGetValue(fileName, out t))
            {
                var t2 = new ReplicationDegrees(t.ActualDegree, t.WantedDegree + actualDegree);
                Dict[fileName] = t2;
            }
            else
            {
                Dict[fileName] = new ReplicationDegrees(1, wantedDegree);
            }
        }
    }
}
