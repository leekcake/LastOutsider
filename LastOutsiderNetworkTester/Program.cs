using LastOutsiderNetworkTester.Test;
using System;

namespace LastOutsiderNetworkTester
{
    class Program
    {
        static void Main(string[] args)
        {
            new GameSocketTest().Test();
            Console.WriteLine("End.");
            Console.ReadLine();
        }
    }
}
