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

            public override float NeedAP => 3.5f;

            public override Target ApplyTarget => Target.OneEnemy;
        }

        public class Zunko2ndSkill : Skill
        {
            public override string Name => "리팩토링";

            public override string Descrption => "모든 스파게티 코드에게 {0}(공격력의 1.5배)의 피해를 입힙니다.\r\n버그에게는 각각 30%의 확률로 {1}(공격력의 0.6배)의 피해를 입힙니다.";

            public override float NeedAP => 6f;

            public override Target ApplyTarget => Target.AllEnemy;
        }

        public ProtoZunko()
        {
            TypeData = Type.Developer;
            GradeData = Grade.A;
            Skills = new Skill[] { new Zunko1stSkill(), new Zunko2ndSkill() };
        }
    }
}
