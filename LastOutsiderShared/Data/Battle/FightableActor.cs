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

        /// <summary>
        /// 주어진 데미지에서 상태등으로 인한 데미지 증감을 적용해
        /// 되돌려줍니다
        /// </summary>
        /// <param name="damage">원래 데미지</param>
        /// <returns>실제로 가해야 할 데미지</returns>
        public float CalculationAttackDamage(float damage)
        {
            //TODO: 실질적인 데미지 계산
            return damage;
        }

        /// <summary>
        /// 주어진 데미지에서 상태등으로 인한 데미지 증감을 적용한뒤
        /// 현재 체력을 수치만큼 깍고, 실제로 적용된 데미지를 반환합니다
        /// </summary>
        /// <param name="damage"></param>
        /// <returns>실제로 받은 데미지</returns>
        public float ApplyDamage(float damage)
        {
            //TODO: 실질적인 데미지 계산
            return damage;
        }
    }
}
