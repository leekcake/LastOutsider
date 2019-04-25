using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data.Battle
{
    /// <summary>
    /// 배틀중 자동으로 적용되는 스킬
    /// </summary>
    public abstract class Passive
    {
        public abstract string Name {
            get;
        }

        public abstract string Descrption {
            get;
        }

        public virtual void OnWaveStart()
        {

        }
    }
}
