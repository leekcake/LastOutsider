using LastOutsiderShared.Data.Battle;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [MessagePackObject]
    /// <summary>
    /// 전투에 참가할 수 있는 적 정보
    /// </summary>
    public class Enemy
    {
        [Key(0)]
        public int Index {
            get; set;
        }

        [Key(1)]
        public int Level {
            get; set;
        }

        [Key(2)]
        public Stats Stat {
            get; set;
        }
    }
}
