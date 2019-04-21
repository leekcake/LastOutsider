using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [Serializable]
    /// <summary>
    /// 소모 가능한 자원
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// 다음 자원이 회복되야 하는 시간
        /// 6분 이상이 지나 2번 이상 회복해야 하는경우도 정상적으로 동작
        /// </summary>
        public DateTime NextRecoverTime {
            get; set;
        }

        public int Money {
            get; set;
        }

        public int MoneyRecoveryAmount {
            get; set;
        } = 3;

        public int Electric {
            get; set;
        }

        public int ElectricRecoveryAmount {
            get; set;
        } = 3;

        public int Food {
            get; set;
        }

        public int FoodRecoveryAmount {
            get; set;
        } = 3;

        public int Time {
            get; set;
        }

        public int TimeRecoveryAmount {
            get; set;
        } = 1;
    }
}
