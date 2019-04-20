using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderShared.Data
{
    [Serializable]
    /// <summary>
    /// 라스트 아웃사이더의 유저 계정
    /// </summary>
    public class Account
    {
        public int Id {
            get; set;
        }

        public byte[] AuthToken {
            get; set;
        }
    }
}
