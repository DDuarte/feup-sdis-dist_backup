using System;
using System.Diagnostics;
using System.IO;

namespace Peer
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 6)
            {
                PrintUsage();
                Console.ReadKey();
                return;
            }

            // TODO: Read arguments

            Console.ReadKey();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: {0} <MC:IP> <MC:Port> <MDB:IP> <MDB:PORT> <MDR:IP> <MDR:Port>", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
            Console.WriteLine("  <MC:IP> <MC:Port>: IP multicast address and port of control channel");
            Console.WriteLine("  <MDB:IP> <MDB:Port>: IP multicast address and port of data backup channel");
            Console.WriteLine("  <MDR:IP> <MDR:Port>: IP multicast address and port of data restore channel");
        }
    }
}
