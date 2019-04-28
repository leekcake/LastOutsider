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
        public static readonly string DEVELOPER_KEY = "Character/Developer/";
        public static readonly string QA_KEY = "Character/QA/";
        public static readonly string DESIGNER_KEY = "Character/Designer/";

        /// <summary>
        /// 캐릭터마다 고정인 정보
        /// </summary>
        public abstract class FixedData : FightableActor.FixedData
        {
            #region Enum
            public enum Type
            {
                Developer = 0, //Attacker
                QA = 1, //Defenser
                Designer = 2 //Supporter
            }

            public enum Grade
            {
                SS = 0,
                S = 1,
                A = 2,
                B = 3
            }
            #endregion

            public Type TypeData {
                get; protected set;
            }

            public Grade GradeData {
                get; protected set;
            }

            public Skill[] Skills {
                get; protected set;
            }

            public Passive[] Passives {
                get; protected set;
            }
            public abstract string Name { get; }
            public abstract string Key { get; }
        }

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
