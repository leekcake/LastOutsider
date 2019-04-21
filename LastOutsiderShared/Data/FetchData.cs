using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [MessagePackObject]
    /// <summary>
    /// Bulk data
    /// </summary>
    public class FetchData
    {
        [Key(0)]
        public Resource resource;
    }
}
