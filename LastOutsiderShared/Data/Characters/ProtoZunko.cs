using LastOutsiderShared.Data.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data.Characters
{
    /// <summary>
    /// 프로토 타입용 첫번째 캐릭터
    /// 
    /// 
    /// </summary>
    public class ProtoZunko : Character.FixedData
    {
        public class Zunko1stSkill : Skill
        {
            public override string Name => "디버깅";

            public override string Descrption => "대상에게 {0}(공격력)의 피해를 입힙니다.\r\n대상이 버그인경우 데미지가 50% 증폭됩니다";

            public override string FormattedDescription => string.Format(Descrption, Utils.FloatDisplay(Performer.Stat.Attack));

            public override float NeedAP => 3.5f;

            public override Target ApplyTarget => Target.OneEnemy;

            public override void OnPerformOneEnemy(FightableActor enemy)
            {
                var damage = Performer.Stat.Attack;
                if(enemy.IfEnemy.IsBug)
                {
                    damage *= 1.5f;
                }
                enemy.ApplyDamage(Performer.CalculationAttackDamage(damage));
            }
        }

        public class Zunko2ndSkill : Skill
        {
            public override string Name => "리팩토링";

            public override string Descrption => "모든 스파게티 코드에게 {0}(공격력의 1.5배)의 피해를 입힙니다.\r\n" +
                "버그에게는 각각 30%의 확률로 {1}(공격력의 0.6배)의 피해를 입힙니다.\r\n" +
                "사이드 이펙트 코드를 각각 5%의 확률로 {2}(공격력의 0.3배)만큼 회복합니다.";

            public override string FormattedDescription => string.Format(Descrption, Utils.FloatDisplay(Performer.Stat.Attack * 1.5f), Utils.FloatDisplay(Performer.Stat.Attack * 0.6f), Utils.FloatDisplay(Performer.Stat.Attack * 0.3f));

            public override float NeedAP => 6f;

            public override Target ApplyTarget => Target.AllEnemy;

            public override void OnPerformAllEnemy(FightableActor[] enemies)
            {
                Random random = new Random();
                foreach(FightableActor enemy in enemies)
                {
                    if(enemy.IfEnemy.IsSpaghettiCode)
                    {
                        enemy.ApplyDamage(Performer.Stat.Attack * 1.5f);
                    }
                    else if(enemy.IfEnemy.IsSideEffectCode)
                    {
                        if (random.Next(0, 100) <= 5)
                        {
                            enemy.ApplyHeal(Performer.Stat.Attack * 0.3f);
                        }
                    }
                    else
                    {
                        if(random.Next(0, 100) <= 30)
                        {
                            enemy.ApplyDamage(Performer.Stat.Attack * 0.6f);
                        }
                    }
                }
            }
        }

        public ProtoZunko()
        {
            TypeData = Type.Developer;
            GradeData = Grade.A;
            Skills = new Skill[] { new Zunko1stSkill(), new Zunko2ndSkill() };
        }
    }
}
