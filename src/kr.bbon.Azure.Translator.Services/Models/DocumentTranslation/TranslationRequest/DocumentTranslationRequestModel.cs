using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr.bbon.Azure.Translator.Services.Models.DocumentTranslation.TranslationRequest
{

    public class RequestModel
    {
        /// <summary>
        /// 문서 또는 문서를 포함 하는 폴더의 입력 목록
        /// </summary>
        [Required]
        public IEnumerable<BatchInput> Inputs { get; set; }
    }

    /// <summary>
    /// 입력 일괄 처리 변환 요청에 대한 정의
    /// </summary>
    public class BatchInput
    {
        /// <summary>
        /// 입력 문서의 원본
        /// </summary>
        public SourceInput Source { get; set; }

        /// <summary>
        /// 입력 문서 원본 문자열의 저장소 유형입니다.
        /// <para>
        /// <see cref="StorageInputTypes"/> 형식에서 제공하는 상수를 확인하세요.
        /// </para>
        /// </summary>
        public string StorageType { get; set; } = StorageInputTypes.File;

        /// <summary>
        /// 출력의 대상 위치입니다.
        /// </summary>
        public IEnumerable<TargetInput> Targets { get; set; }
    }

    /// <summary>
    /// 입력 문서의 원본
    /// </summary>
    public class SourceInput
    {
        public Filter Filter { get; set; }

        /// <summary>
        /// <para>언어코드</para>
        /// <para>
        /// 언어 코드 지정 하지 않을 경우 문서에서 자동 검색을 수행 합니다.
        /// </para>
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// 문서를 포함 하는 폴더/컨테이너 또는 단일 파일의 위치
        /// </summary>
        [Required]
        public string SourceUrl { get; set; }

        /// <summary>
        /// <para>
        /// 저장소 원본
        /// </para>
        /// <para>
        /// Storage source
        /// </para>
        /// <para>
        /// <see cref="StorageSources"/> 형식에서 제공하는 상수를 확인하세요.
        /// </para>
        /// </summary>
        public string StorageSource { get; set; } = StorageSources.AzureBlob;
    }

    /// <summary>
    /// 완성 된 번역 문서의 대상
    /// </summary>
    public class TargetInput
    {
        /// <summary>
        /// 번역 요청에 대 한 범주/사용자 지정 시스템
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 용어집 목록
        /// </summary>
        public IEnumerable<Glossary> Glossaries { get; set; }

        /// <summary>
        /// 대상 언어
        /// </summary>
        [Required]
        public string Language { get; set; }

        /// <summary>
        /// 저장소 원본
        /// <para>
        /// <see cref="StorageSources"/> 형식에서 제공하는 상수를 확인하세요.
        /// </para>
        /// </summary>
        public string StorageSource { get; set; } = StorageSources.AzureBlob;

        /// <summary>
        /// 문서를 포함 하는 폴더/컨테이너의 위치
        /// </summary>
        [Required]
        public string TargetUrl { get; set; }
    }

    public class Filter
    {
        /// <summary>
        /// 변환을 위해 소스 경로에서 문서를 필터링 하는 대/소문자를 구분 하는 접두사 문자열입니다. 
        /// <para>
        /// 예를 들어 Azure storage blob Uri를 사용 하는 경우 접두사를 사용 하 여 번역을 위해 하위 폴더를 제한 합니다.
        /// </para>
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// 변환을 위해 소스 경로에서 문서를 필터링 하는 대/소문자를 구분 하는 접미사 문자열입니다. 
        /// <para>
        /// 일반적으로 파일 확장명에 사용 됩니다.
        /// </para>
        /// </summary>
        public string Suffix { get; set; }
    }


    public class Glossary
    {
        /// <summary>
        /// 형식
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 용어집의 위치입니다. 
        /// <para>Format 매개 변수가 제공 되지 않은 경우 파일 확장명을 사용 하 여 서식 지정을 추출 합니다.</para>
        /// <para>용어집에 번역 언어 쌍이 없으면 적용 되지 않습니다.</para>
        /// </summary>
        public string GlossaryUrl { get; set; }

        /// <summary>
        /// 저장소 원본
        /// <para>
        /// <see cref="StorageSources"/> 형식에서 제공하는 상수를 확인하세요.
        /// </para>
        /// </summary>
        public string StorageSource { get; set; } = StorageSources.AzureBlob;

        /// <summary>
        /// 버전
        /// </summary>
        public string Version { get; set; }
    }

    /// <summary>
    /// 저장소 원본
    /// </summary>
    public class StorageSources
    {
        public static string AzureBlob = "AzureBlob";
    }


    /// <summary>
    /// 입력 문서 원본 문자열의 저장소 유형입니다.
    /// </summary>
    public class StorageInputTypes
    {
        public static string File = "File";
        public static string Folder = "Folder";
    }

    public class ResponseModel
    {
        /// <summary>
        /// Translation job id
        /// </summary>
        public string Id { get; set; }
    }

    //public class ErrorResponseModel
    //{
    //    public ErrorModel<int> Error { get; set; }
    //}
}