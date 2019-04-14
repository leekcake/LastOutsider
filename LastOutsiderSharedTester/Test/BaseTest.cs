using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderSharedTester.Test
{
    public abstract class BaseTest
    {
        protected void Assert(object a, object b)
        {
            if (a != b)
            {
                throw new Exception($"Assert Failed: {a} != {b}");
            }
        }

        protected void Assert(object[] a, object[] b)
        {
            if(a.Length != b.Length)
            {
                throw new Exception($"Assert Array Failed, Length mismatch: {a.Length} != {b.Length}");
            }

            for(int i = 0; i < a.Length; i++)
            {
                if(a[i] != b[i])
                {
                    throw new Exception($"Assert Array Failed, item at ${0} is mismatch: {a[i]} != {b[i]}");
                }
            }
        }

        public void Test()
        {
            Console.WriteLine($"Start Test: {Name}");

            try {
                TestInternal();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine($"End Test: {Name}");
        }

        protected abstract string Name {
            get;
        }
        protected abstract void TestInternal();
    }
}
