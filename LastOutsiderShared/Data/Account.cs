using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [MessagePackObject]
    /// <summary>
    /// 라스트 아웃사이더의 유저 계정
    /// </summary>
    public class Account
    {
        [Key(0)]
        public int Id {
            get; set;
        }

        [Key(1)]
        public byte[] AuthToken {
            get; set;
        }
    }
}
