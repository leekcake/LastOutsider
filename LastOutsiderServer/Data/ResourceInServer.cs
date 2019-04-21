using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LastOutsiderServer.Data
{
    /// <summary>
    /// 리소스의 자연회복은 ServerDataBase.GetResource 에서 처리
    /// </summary>
    public class ResourceInServer : Resource
    {
        /// <summary>
        /// 이 리소스를 사용하는 유저 데이터
        /// </summary>
        public int UserId {
            get; set;
        }
    }
}
