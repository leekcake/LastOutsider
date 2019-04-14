using System;
using LastOutsiderSharedTester.Test;

namespace LastOutsiderSharedTester
{
    class Program
    {
        static void Main(string[] args)
        {
            new EncryptHelperTest().Test();

            Console.WriteLine("End of Main, Enter to End :)");
            Console.ReadLine();
        }
    }
}
