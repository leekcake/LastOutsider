using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data.Battle
{
    [MessagePackObject]
    /// <summary>
    /// 전투에 참가할 수 있는 객체
    /// 
    /// </summary>
    public class FightableActor
    {
        /// <summary>
        /// 이 객체를 구분하는 키
        /// </summary>
        [Key(0)]
        public string Key {
            get; set;
        }

        /// <summary>
        /// 이 객체의 레벨
        /// </summary>
        [Key(1)]
        public int Level {
            get; set;
        }

        /// <summary>
        /// 현재 체력
        /// </summary>
        [Key(2)]
        public int HP {
            get; set;
        }

        /// <summary>
        /// 현재 행동력
        /// </summary>
        [Key(3)]
        public float AP {
            get; set;
        }

        /// <summary>
        /// 이 캐릭터의 성능
        /// </summary>
        [Key(4)]
        public Stats Stat {
            get; set;
        }

        [Key(5)]
        public List<Status> Statuses {
            get; set;
        }
    }
}
