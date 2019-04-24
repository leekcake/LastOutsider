using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data.Battle
{
    /// <summary>
    /// 전투에 참여할 수 있는 캐릭터의 스탯
    /// </summary>
    [MessagePackObject]
    public class Stats
    {
        /// <summary>
        /// 최대 체력
        /// </summary>
        [Key(0)]
        public int MaxHP {
            get; set;
        }

        /// <summary>
        /// 공격력
        /// </summary>
        [Key(1)]
        public float Attack {
            get; set;
        }
        /// <summary>
        /// 치명타 확률
        /// </summary>
        [Key(2)]
        public float Critical {
            get; set;
        }
        /// <summary>
        /// 방어력
        /// </summary>
        [Key(3)]
        public float Defense {
            get; set;
        }

        /// <summary>
        /// 명중률
        /// </summary>
        [Key(4)]
        public float Hit {
            get; set;
        }
        /// <summary>
        /// 회피 확률
        /// </summary>
        [Key(5)]
        public float Evade {
            get; set;
        }

        /// <summary>
        /// 행동속도
        /// </summary>
        [Key(6)]
        public float Speed {
            get; set;
        }
    }
}
