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
        #region Helper Class
        public class CharacterHelper
        {
            private FightableActor fightableActor;

            public CharacterHelper(FightableActor fightableActor)
            {
                this.fightableActor = fightableActor;
            }

            public bool IsDeveloper {
                get {
                    return fightableActor.Key.StartsWith(Character.DEVELOPER_KEY);
                }
            }

            public bool IsQA {
                get {
                    return fightableActor.Key.StartsWith(Character.QA_KEY);
                }
            }

            public bool IsDesigner {
                get {
                    return fightableActor.Key.StartsWith(Character.DESIGNER_KEY);
                }
            }
        }

        public class EnemyHelper
        {
            private FightableActor fightableActor;

            public EnemyHelper(FightableActor fightableActor)
            {
                this.fightableActor = fightableActor;
            }

            public bool IsBug {
                get {
                    return fightableActor.Key.StartsWith(Enemy.BUG_KEY);
                }
            }

            public bool IsSpaghettiCode {
                get {
                    return fightableActor.Key.StartsWith(Enemy.SPAGHETTI_CODE_KEY);
                }
            }

            public bool IsSideEffectCode {
                get {
                    return fightableActor.Key.StartsWith(Enemy.SIDE_EFFECT_CODE_KEY);
                }
            }
        }
        #endregion

        #region Fixed Data
        public interface FixedData
        {
            string Name {
                get;
            }
        }

        public static readonly Dictionary<string, FixedData> FixedDatas = new Dictionary<string, FixedData>();
        #endregion

        /// <summary>
        /// 이 객체를 구분하는 키
        /// </summary>
        [Key(0)]
        public string Key {
            get; set;
        }

        [IgnoreMember]
        public string Name {
            get {
                FixedData fixedData = null;
                FixedDatas.TryGetValue(Key, out fixedData);
                if(fixedData == null)
                {
                    return "더미";
                }
                return fixedData.Name;
            }
        }

        [IgnoreMember]
        public bool IsCharacter {
            get {
                return Key.StartsWith("Character/");
            }
        }

        [IgnoreMember]
        public CharacterHelper IfCharacter {
            get; private set;
        }

        [IgnoreMember]
        public EnemyHelper IfEnemy {
            get; private set;
        }

        [IgnoreMember]
        public bool IsEnemy {
            get {
                return Key.StartsWith("Enemy/");
            }
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
        /// <returns>실제로 적용된 데미지</returns>
        public int ApplyDamage(float damage)
        {
            //TODO: 실질적인 데미지 계산

            int convertedDamage = (int)damage;
            HP -= convertedDamage;
            return convertedDamage;
        }

        /// <summary>
        /// 주어진 힐량에서 상태등으로 인한 증감을 적용한뒤
        /// 현재 체력을 수치만큼 회복시키고, 실제로 적용된 힐량을 반환합니다
        /// </summary>
        /// <param name="heal">힐량</param>
        /// <returns>실제로 적용된 힐량</returns>
        public int ApplyHeal(float heal)
        {
            //TODO: 실질적인 힐량 계산
            int convertedHeal = (int)heal;
            HP += convertedHeal;
            return convertedHeal;
        }
    }
}
