# kr.bbon.Azure.Translator.Services Package

[![](https://img.shields.io/nuget/v/kr.bbon.Azure.Translator.Services)](https://www.nuget.org/packages/kr.bbon.Azure.Translator.Services) [![](https://img.shields.io/nuget/dt/kr.bbon.Azure.Translator.Services)](https://www.nuget.org/packages/kr.bbon.Azure.Translator.Services) [![publish to nuget](https://github.com/bbonkr/kr.bbon.Azure.Translator.Services/actions/workflows/main.yml/badge.svg)](https://github.com/bbonkr/kr.bbon.Azure.Translator.Services/actions/workflows/main.yml)

## 개요

Azure Translator 를 사용한 번역 서비스입니다.

[nuget: Azure.AI.Translation.Document](https://www.nuget.org/packages/Azure.AI.Translation.Document) 패키지가 출시되었습니다.

해당 패키지를 사용하시는 것으로 권장합니다.

## 사전요구사항

* [Azure 계정](https://azure.microsoft.com/free)
* [Azure Translator](https://azure.microsoft.com/ko-kr/services/cognitive-services/translator/)
  * [Document Translation (Preview)](https://docs.microsoft.com/ko-kr/azure/cognitive-services/translator/document-translation/get-started-with-document-translation) 사용 신청
* [Azure Storage Account](https://docs.microsoft.com/ko-kr/azure/storage/common/storage-account-overview)

## 요구사항

* .NET 5

## 번역

### 문자열 번역

짧은 문자열에 대한 실시간 번역을 제공합니다.

Azure Translator 는 번역할 문자열과 번역되는 문자열 전체 길이에 대한 제약사항이 존재합니다. 

> 제한사항
> 
> [요청제한](https://docs.microsoft.com/ko-kr/azure/cognitive-services/translator/request-limits) 문서에서 제한사항을 확인하세요.


`ITextTranslatorService` 인터페이스를 구현하는 `TextTranslatorService` 클래스에서 기능을 제공합니다.



### 문서 번역

실시간 번역으로 처리할 수 없는 긴 문자열 혹은 파일 내용에 대한 번역을 제공합니다.

[Document Translation (Preview)](https://docs.microsoft.com/ko-kr/azure/cognitive-services/translator/document-translation/get-started-with-document-translation) 기능을 사용 신청해야 합니다.

 무료 가격 정책에서는 동작하지 않습니다. 

> 확인사항
> 
> [관련 이슈: MicrosoftDocs/azure-docs #71988](https://github.com/MicrosoftDocs/azure-docs/issues/71988#event-4449870111)


 
`IDocumentTranslationService` 인터페이스를 구현하는 `DocumentTranslationService` 클래스에서 기능을 제공합니다.


## 설정 및 구성

### 설정

appsettings.json 에서 설정을 제공합니다.

```json
{
    "Translator": {
        "Endpoint": "endpoint here",
        "SubscriptionKey": "subscription key here",
        "Region": "region here",
        "ResourceName": "name of resource",
        "SourceBlobContainerName": "Source blob container name",
        "TargetBlobContainerName": "Target blob container name"
    },
    "AzureStorage": {
        "ConnectionString": "azure storage connection string here"
    }
}
```

구성파일에 작성된 내용은 `AzureStorageOptions` 클래스와 `AzureTranslatorConnectionOptions` 클래스의 인스턴스에 바인딩되어 사용됩니다.

### 구성

```csharp

IConfiguration Configuration { get;}

// ConfigureServices method in Startup class
public void ConfigureServices(IServiceCollection services)
{
    // register dependencies    
    services.AddAzureTranslatorServices(Configuration);
}
```

## 예제

예제는 [bbonkr/Sample.Azure.Translator](https://github.com/bbonkr/Sample.Azure.Translator) 페이지를 참조하세요.

## 변경사항

### v1.1.0

잘 동작하던 문서 번역기 서비스가 서버 오류가 발생하고 있습니다.

번역 원본, 대상으로 BLOB 항목을 사용하고 있었는데, 원본, 대상을 컨테이너로 지정하니 문제가 해결되었습니다.

문서 번역 서비스에 변경사항이 있는지에 대한 설명은 현재 찾을 수 없습니다. (2021-08-04)

문제없는 동작을 위해 번역 원본, 대상으로 컨테이너 SAS URI 를 지정하도록 변경합니다.

RESTful api 끝점을 v1.0-preview.1 에서 v1.0 으로 변경합니다.