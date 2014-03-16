using System.IO;
using BplusDotNet;

namespace Peer
{
    public static class BplusTreeExt
    {
        public static BplusTree OpenBplusTree(string treefileName, string blockfileName, int keyLength)
        {
            if (File.Exists(treefileName) && File.Exists(blockfileName))
                return BplusTree.ReOpen(treefileName, blockfileName);
            return BplusTree.Initialize(treefileName, blockfileName, keyLength);
        }

        public static string Get(this BplusTree tree, string key)
        {
            try
            {
                return tree[key];
            }
            catch (BplusTreeKeyMissing)
            {
                return null;
            }
        }
    }
}
