using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using DBS;

namespace PeerGUI
{
    public partial class ChunksStoreForm : Form
    {
        public ChunksStoreForm()
        {
            InitializeComponent();

            Observable.Interval(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(_ => FillChunksListView());
        }

        private int _prevCount;

        private void FillChunksListView()
        {
            var currCount = Core.Instance.ChunkPeers.GetChunkPeersCount();
            if (_prevCount == currCount)
                return; // no update required
            _prevCount = currCount;

            chunksListView.BeginUpdate();
            try
            {
                chunksListView.Items.Clear();
                try
                {
                    foreach (var chunkPeer in Core.Instance.ChunkPeers)
                    {
                        var chunk = chunkPeer.Chunk;
                        int wantedDeg, actualDeg;
                        if (!Core.Instance.ChunkPeers.TryGetDegrees(chunk, out wantedDeg, out actualDeg))
                            continue;

                        var ip = new IPAddress(chunkPeer.IP).ToString();
                        var actualDegStr = actualDeg.ToString(CultureInfo.InvariantCulture);
                        var wantedDegStr = wantedDeg.ToString(CultureInfo.InvariantCulture);
                        chunksListView.Items.AddWithTextAndSubItems(chunk, ip, actualDegStr, wantedDegStr);
                    }
                }
                catch (Exception ex)
                {
                    Core.Instance.Log.Error("FillChunksListView", ex);
                }
            }
            finally
            {
                chunksListView.EndUpdate();
            }
        }
    }

    internal static class ListViewItemCollectionExtender
    {
        internal static void AddWithTextAndSubItems(this ListView.ListViewItemCollection col,
            string text, params string[] subItems)
        {
            var item = new ListViewItem(text);
            foreach (var subItem in subItems)
                item.SubItems.Add(subItem);
            col.Add(item);
        }
    }
}
