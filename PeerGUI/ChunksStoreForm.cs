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
            FillChunksListView();

            var observable = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Core.Instance.ChunkPeers.ObservableCollection, "CollectionChanged");
            var subs = observable.DistinctUntilChanged()
                .Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(OnChanged);
        }

        private void OnChanged(EventPattern<NotifyCollectionChangedEventArgs> pattern)
        {
            var e = pattern.EventArgs;

            if (e == null || ((e.NewItems == null || e.NewItems.Count == 0) && (e.OldItems == null || e.OldItems.Count == 0)))
                return;

            FillChunksListView();
        }

        private void FillChunksListView()
        {
            chunksListView.Items.Clear();
            foreach (var chunkPeer in Core.Instance.ChunkPeers.ObservableCollection)
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
