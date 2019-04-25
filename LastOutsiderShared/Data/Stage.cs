using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [MessagePackObject]
    /// <summary>
    /// 전투가 진행되는 스테이지 정보
    /// </summary>
    public class Stage
    {
        [Key(0)]
        public int WaveCount {
            get; set;
        }

        [Key(1)]
        public List<Enemy[]> Enemies;
    }
}
