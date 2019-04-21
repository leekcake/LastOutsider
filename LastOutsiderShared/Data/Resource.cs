using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [MessagePackObject]
    /// <summary>
    /// 소모 가능한 자원
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// 다음 자원이 회복되야 하는 시간
        /// 6분 이상이 지나 2번 이상 회복해야 하는경우도 정상적으로 동작
        /// </summary>
        [Key(0)]
        public DateTime NextRecoverTime {
            get; set;
        }

        [Key(1)]
        public int Money {
            get; set;
        }

        [Key(2)]
        public int MoneyRecoveryAmount {
            get; set;
        } = 3;

        [Key(3)]
        public int Electric {
            get; set;
        }

        [Key(4)]
        public int ElectricRecoveryAmount {
            get; set;
        } = 3;

        [Key(5)]
        public int Food {
            get; set;
        }

        [Key(6)]
        public int FoodRecoveryAmount {
            get; set;
        } = 3;

        [Key(7)]
        public int Time {
            get; set;
        }

        [Key(8)]
        public int TimeRecoveryAmount {
            get; set;
        } = 1;
    }
}
