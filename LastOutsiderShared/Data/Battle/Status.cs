using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace LastOutsiderShared.Data.Battle
{
    /// <summary>
    /// 상태(이상) 정보
    /// </summary>
    [MessagePackObject]
    public class Status
    {
        /// <summary>
        /// 상태 정보를 토대로 데미지나 스탯을 조절해주는 클래스
        /// </summary>
        public interface StatusProcesser
        {
            /// <summary>
            /// 주어진 스탯을 보정합니다
            /// </summary>
            /// <param name="statuses">적용할 상태(복수)</param>
            /// <param name="original">원래 스탯</param>
            /// <returns>상태가 변형한 스탯</returns>
            Stats ProcessStat(Status[] statuses, Stats original);

            /// <summary>
            /// 데미지를 상대에게 입히기 전에 재조정합니다
            /// </summary>
            /// <param name="statuses">적용할 상태(복수)</param>
            /// <param name="original">원래 데미지</param>
            /// <returns>조정된 데미지</returns>
            int ProcessDamageBeforeAttack(Status[] statuses, int original);

            /// <summary>
            /// 데미지를 입기전에 재조정합니다
            /// </summary>
            /// <param name="statuses">적용할 상태(복수)</param>
            /// <param name="original">원래 데미지</param>
            /// <returns>조정된 데미지</returns>
            int ProcessDamageBeforeHit(Status[] statuses, int original);

            /// <summary>
            /// 시작할때 입힐 데미지를 계산합니다 (독 등)
            /// </summary>
            /// <param name="statuses">적용할 상태(복수)</param>
            /// <returns>입을 데미지</returns>
            int ProcessDamageOnStart(Status[] statuses);
        }

        /// <summary>
        /// 상태 이상 구분용
        /// </summary>
        [Key(0)]
        public int Id {
            get; set;
        }

        /// <summary>
        /// 이 상태가 제거되기 까지 남은 라운드
        /// </summary>
        [Key(1)]
        public int LeftRound {
            get; set;
        }

        /// <summary>
        /// 이 상태를 처리하기 위해 프로세서가 넣은 데이터
        /// </summary>
        [Key(2)]
        public byte[] ExtraData {
            get; set;
        }

        /// <summary>
        /// 이 상태를 처리하기 위해 프로세서가 데이터를 오브젝트 형식으로 저장한것
        /// 이 오브젝트는 ExtraData에서 언제든지 만들 수 있기때문에 전달하지 않음
        /// Field는 데이터베이스에 저장되지 않으므로 데이터베이스에도 저장되지 않음
        /// </summary>
        [IgnoreMember]
        public object ExtraDataObject;
    }
}
