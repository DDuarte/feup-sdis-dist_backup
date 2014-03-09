using System;
using System.IO;
using System.Text;
using DBS;

namespace Peer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var msg = Message.BuildPutChunkMessage(1, 0, new byte[] { 1, 2, 3}, 50, 5, new byte[] { 65, 65, 65 });
            var byteArray = msg.Serialize();

            var charArray = new char[Encoding.ASCII.GetCharCount(byteArray)];
            Encoding.ASCII.GetChars(byteArray, 0, charArray.Length, charArray, 0);
            Console.WriteLine(charArray);
            Console.ReadKey();
        }
    }
}
