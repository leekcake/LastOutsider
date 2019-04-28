using LastOutsiderShared.Data.Battle;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data.Enemies
{
    public class MinorBug : Enemy.FixedData
    {
        public override string Key => Enemy.BUG_KEY + "MinorBug";

        public override string Name => "사소한 버그";

        public class MinorBug1stSkill : Skill
        {
            public override string Name => "버그다!";

            public override string Descrption => "자주 사용하지 않는 부분에서 작은 문제를 일으킵니다\r\n" +
                "대상에게 {0}(공격력)의 피해를 입힙니다.\r\n" +
                "대상이 디자이너인경우 {1}(공격력의 1.5배)의 피해를 입힙니다";

            public override string FormattedDescription => string.Format(Descrption, 
                Utils.FloatDisplay(Performer.Stat.Attack),
                Utils.FloatDisplay(Performer.Stat.Attack * 1.5f));

            public override float NeedAP => 5;

            public override Target ApplyTarget => Target.OneEnemy;

            public override void OnPerformOneEnemy(FightableActor enemy)
            {
                if( enemy.IfCharacter.IsDesigner )
                {
                    PrintAttackLog(enemy, enemy.ApplyDamage(Performer.Stat.Attack * 1.5f));
                }
                else
                {
                    PrintAttackLog(enemy, enemy.ApplyDamage(Performer.Stat.Attack));
                }
            }
        }

        public class MinorBugPassive : Passive
        {
            public override string Name => "자주 사용하지 않음";

            public override string Descrption => "이 버그는 자주 사용되지 않는 부분에서 발생했습니다\r\n" +
                "이 버그는 QA의 공격을 받기 전까지 데미지와 받는 피해가 50% 감소합니다";

            //TODO: 이 패시브 구현하기
        }

        public MinorBug()
        {
            Skills = new Skill[] { new MinorBug1stSkill() };
            Passives = new Passive[] { new MinorBugPassive() };
        }
    }
}
