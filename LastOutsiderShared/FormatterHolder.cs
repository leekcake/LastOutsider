using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LastOutsiderShared
{
    public static class FormatterHolder
    {
        [ThreadStatic]
        private static BinaryFormatter _binaryFormatter = null;
        public static BinaryFormatter binaryFormatter {
            get {
                if(_binaryFormatter == null)
                {
                    _binaryFormatter = new BinaryFormatter();
                }
                return _binaryFormatter;
            }
        }
    }
}
