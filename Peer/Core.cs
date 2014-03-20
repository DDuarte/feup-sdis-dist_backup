using System;

namespace Peer
{
    public sealed class Core
    {
        private static readonly Lazy<Core> Lazy = new Lazy<Core>(() => new Core());

        public static Core Instance { get { return Lazy.Value; } }

        private Core()
        {
            Store = new PersistentStore();
            Rnd = new Random();
        }

        public PersistentStore Store { get; private set; }
        public Random Rnd { get; private set; }
    }
}
