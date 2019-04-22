using LastOutsiderServerTester.Test;
using System;

namespace LastOutsiderServerTester
{
    class Program
    {
        static void Main(string[] args)
        {
            new ResourceRecoveryTest().Test();

            Console.ReadLine();
        }
    }
}
