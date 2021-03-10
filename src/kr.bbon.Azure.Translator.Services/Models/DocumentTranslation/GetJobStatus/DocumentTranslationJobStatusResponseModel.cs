using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr.bbon.Azure.Translator.Services.Models.DocumentTranslation.GetJobStatus
{

    public class JobStatus
    {
        public const string Cancelled = "Cancelled";
        public const string Cancelling = "Cancelling";
        public const string Failed = "Failed";
        public const string NotStarted = "NotStarted";
        public const string Running = "Running";
        public const string Succeeded = "Succeeded";
        public const string ValidationFailed = "ValidationFailed";
    }

    public class JobStatusResponseModel
    {
        /// <summary>
        /// 작업의 Id입니다.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 작업을 만든 날짜 시간
        /// </summary>
        public string CreatedDateTimeUtc { get; set; }

        /// <summary>
        /// 작업 상태가 업데이트 된 날짜 시간
        /// </summary>
        public string LastActionDateTimeUtc { get; set; }

        /// <summary>
        /// 작업 또는 문서에 대한 가능한 상태 목록
        /// <para>
        /// <see cref="JobStatus"/> 에 정의된 상태 문자열을 참조하세요.
        /// </para>
        /// </summary>
        public string Status { get; set; }

        public StatusSummary Summary { get; set; }

        /// <summary>
        /// 여기에는 오류 코드, 메시지, 세부 정보, 대상 및 자세한 설명이 포함 된 내부 오류와 함께 외부 오류가 있습니다.
        /// </summary>
        public ErrorV2 Error { get; set; }
    }

    public class ErrorV2 : ErrorModel<string>
    {
        /// <summary>
        /// 오류의 소스를 가져옵니다.
        /// <para>
        /// 예를 들어 잘못 된 문서가 있는 경우 "문서" 또는 "문서 id"가 될 수 있습니다.
        /// </para>
        /// </summary>
        public string Target { get; set; }
    }

    public class StatusSummary
    {
        /// <summary>
        /// 취소 횟수
        /// </summary>
        public int Cancelled { get; set; }

        /// <summary>
        /// 실패 수
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// 진행 중인 수
        /// </summary>
        public int InProgress { get; set; }

        /// <summary>
        /// 아직 시작 되지 않은 수
        /// </summary>
        public int NotYetStarted { get; set; }

        /// <summary>
        /// 성공 수
        /// </summary>
        public int Success { get; set; }

        /// <summary>
        /// 총 개수
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// API로 청구 되는 총 문자
        /// </summary>
        public int TotalCharacterCharged { get; set; }
    }
}