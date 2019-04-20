using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LastOutsiderShared
{
    public static class ExceptionUtils
    {
        public static void OnException(Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}
