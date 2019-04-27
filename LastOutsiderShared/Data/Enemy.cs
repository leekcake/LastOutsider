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
        public static readonly string BUG_KEY = "Enemy/Bug/";
        public static readonly string SPAGHETTI_CODE_KEY = "Enemy/Spaghetti Code/";
        public static readonly string SIDE_EFFECT_CODE_KEY = "Enemy/Side Effect Code/";

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
