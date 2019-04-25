using LastOutsiderShared.Data.Battle;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [MessagePackObject]
    /// <summary>
    /// 플레이어가 사용하는 캐릭터(데브로이드)
    /// </summary>
    public class Character
    {
        [Key(0)]
        public int Id {
            get; set;
        }

        [Key(1)]
        public int Index {
            get; set;
        }

        [Key(2)]
        public int Level {
            get; set;
        }

        [Key(3)]
        public int Exp {
            get; set;
        }

        [Key(4)]
        public int HP {
            get; set;
        }

        [Key(8)]
        public byte StressLevel = 0;

        [Key(5)]
        public int MaxHP {
            get; set;
        }

        [Key(6)]
        public float FavorPoint {
            get; set;
        }

        [IgnoreMember]
        public Stats Stat {
            get {
                return PureStat;
            }
        }

        [Key(7)]
        public Stats PureStat {
            get; set;
        }
    }
}
