using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data.Battle
{
    /// <summary>
    /// 배틀중 사용할 수 있는 스킬
    /// </summary>
    public abstract class Skill
    {
        public FightableActor Performer {
            get; set;
        }

        /// <summary>
        /// 스킬의 대상
        /// </summary>
        public enum Target
        {
            Self = 0, //나 자신
            OneEnemy = 1, //적 하나
            OneAlly = 2, //아군 하나
            AllEnemy = 3, //적 전체
            AllAlly = 4, //아군 전체
            EnemyOrAlly = 5, //적 혹은 아군
            EnemyAndAlly = 6 //적 한명과 아군 한명
        }

        public abstract string Name {
            get;
        }

        public abstract string Descrption {
            get;
        }

        public abstract string FormattedDescription {
            get;
        }

        public abstract float NeedAP {
            get;
        }

        public abstract Target ApplyTarget {
            get;
        }

        public virtual void OnPerformSelf()
        {
            throw new NotImplementedException();
        }

        public virtual void OnPerformOneEnemy(FightableActor enemy)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPerformOneAlly(FightableActor ally)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPerformAllEnemy(FightableActor[] enemies)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPerformAllAlly(FightableActor[] allies)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPerformEnemyOrAlly(FightableActor actor, bool actorIsEnemy)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPerformEnemyAndAlly(FightableActor enemy, FightableActor ally)
        {
            throw new NotImplementedException();
        }
    }
}
