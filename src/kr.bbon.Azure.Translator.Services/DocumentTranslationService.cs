using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using kr.bbon.Azure.Translator.Services.Models;
using kr.bbon.Azure.Translator.Services.Models.DocumentTranslation;
using kr.bbon.Azure.Translator.Services.Models.DocumentTranslation.GetJobStatus;
using kr.bbon.Azure.Translator.Services.Models.DocumentTranslation.TranslationRequest;

namespace kr.bbon.Azure.Translator.Services
{
    public interface IDocumentTranslationService
    {
        Task<ResponseModel> RequestTranslation(RequestModel model, CancellationToken cancellationToken = default);

        Task<JobStatusResponseModel> GetJobStatusAsync(string id, CancellationToken cancellationToken = default);
    }

    public class DocumentTranslationService : TranslatorServiceBase, IDocumentTranslationService
    {
        public DocumentTranslationService(
            IOptionsMonitor<AzureTranslatorConnectionOptions> azureTranslatorConnectionOptionsAccessor,
            ILoggerFactory loggerFactory)
            : base(azureTranslatorConnectionOptionsAccessor)
        {
            logger = loggerFactory.CreateLogger<DocumentTranslationService>();
        }

        public async Task<ResponseModel> RequestTranslation(RequestModel model, CancellationToken cancellationToken = default)
        {
            ValidateAzureTranslateConnectionOptions();
            ValidateRequestbody(model);

            ResponseModel result = null;

            var requestBody = SerializeToJson(model);

            using (var client = new HttpClient())
            {

                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(GetApiUrl());

                    request.Headers.Add(OCP_APIM_SUBSCRIPTION_KEY, options.SubscriptionKey);
                    request.Headers.Add(OCP_APIM_SUBSCRIPTION_REGION, options.Region);

                    request.Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE_VALUE);

                    var response = await client.SendAsync(request, cancellationToken);

                    var resultJson = await response.Content?.ReadAsStringAsync();
                    var responseContentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    };

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation($"${Tag} The request has been processed. => Translated.");

                        if (response.Headers.Contains("Operation-Location"))
                        {
                            result = response.Headers.GetValues("Operation-Location").Select(operationLocation => new ResponseModel
                            {
                                Id = operationLocation,
                            }).FirstOrDefault();
                        }
                        else
                        {
                            var code = "No operation location";
                            var message = "Translation operation could not find.";
                            throw new ApiHttpStatusException<ErrorModel<string>>(
                                HttpStatusCode.NotAcceptable,
                                message,
                                new ErrorModel<string>
                                {
                                    Code = code,
                                    Message = message,
                                }
                            );
                        }
                    }
                    else
                    {
                        ErrorResponseModel errorResult;

                        if (responseContentType.EndsWith("/json", StringComparison.OrdinalIgnoreCase))
                        {
                            errorResult = JsonSerializer.Deserialize<ErrorResponseModel>(resultJson);

                            logger.LogInformation($"${Tag} The request does not has been processed. => Not Translated.");
                        }
                        else
                        {
                            errorResult = new ErrorResponseModel
                            {
                                Error = new ErrorModel<int>
                                {
                                    Code = (int)response.StatusCode,
                                    Message = response.ReasonPhrase,
                                },
                            };

                            logger.LogInformation($"${Tag} The request does not has been processed. => Server error.");
                        }

                        throw new ApiHttpStatusException<ErrorModel<int>>(response.StatusCode, errorResult.Error.Message, errorResult.Error);
                    }
                }
            }

            return result;
        }

        public async Task<JobStatusResponseModel> GetJobStatusAsync(string id, CancellationToken cancellationToken = default)
        {
            var message = "";
            JobStatusResponseModel result;

            ValidateAzureTranslateConnectionOptions();

            if (string.IsNullOrWhiteSpace(id))
            {
                message = $"{nameof(id)} is required";
                throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.BadRequest, message, new ErrorModel<int>
                {
                    Code = (int)HttpStatusCode.BadRequest,
                    Message = message,
                });
            }     

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri($"{GetApiUrl()}/{id}");

                    request.Headers.Add(OCP_APIM_SUBSCRIPTION_KEY, options.SubscriptionKey);
                    request.Headers.Add(OCP_APIM_SUBSCRIPTION_REGION, options.Region);

                    var response = await client.SendAsync(request, cancellationToken);

                    var resultJson = await response.Content?.ReadAsStringAsync();
                    var responseContentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;                  

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation($"${Tag} The request has been processed. => Translated.");

                        result = JsonSerializer.Deserialize<JobStatusResponseModel>(resultJson);
                    }
                    else
                    {
                        ErrorResponseModel errorResult;

                        if (responseContentType.EndsWith("/json", StringComparison.OrdinalIgnoreCase))
                        {
                            errorResult = JsonSerializer.Deserialize<ErrorResponseModel>(resultJson);

                            logger.LogInformation($"${Tag} The request does not has been processed. => Not Translated.");
                        }
                        else
                        {
                            errorResult = new ErrorResponseModel
                            {
                                Error = new ErrorModel<int>
                                {
                                    Code = (int)response.StatusCode,
                                    Message = response.ReasonPhrase,
                                }
                            };

                            logger.LogInformation($"${Tag} The request does not has been processed. => Server error.");
                        }

                        throw new ApiHttpStatusException<ErrorModel<int>>(response.StatusCode, errorResult.Error.Message, errorResult.Error);
                    }
                }
            }

            return result;
        }

        private void ValidateRequestbody(Models.DocumentTranslation.TranslationRequest.RequestModel model)
        {
            var errors = new List<string>();

            if (model.Inputs == null || model.Inputs.Count() == 0)
            {
                errors.Add("Inputs is required.");
            }

            if (model.Inputs.Any(x => string.IsNullOrWhiteSpace(x.Source.SourceUrl)))
            {
                errors.Add("Source url is required.");
            }

            if (model.Inputs.Any(x => x.Targets.Any(y => string.IsNullOrWhiteSpace(y.TargetUrl))))
            {
                errors.Add("Target url is required.");
            }

            if (errors.Count > 0)
            {
                throw new ApiHttpStatusException<IEnumerable<string>>(HttpStatusCode.BadRequest, errors.FirstOrDefault(), errors);
            }
        }

        protected override string GetBaseUrl()
        {
            return $"https://{options.ResourceName}.cognitiveservices.azure.com";
        }

        protected override void ValidateConditionAzureTranslateConnectionOptions()
        {
            base.ValidateConditionAzureTranslateConnectionOptions();

            if (string.IsNullOrWhiteSpace(options.ResourceName))
            {
                errorMessages.Add($"{nameof(AzureTranslatorConnectionOptions.ResourceName)} is required");
            }
        }

        protected override string Route { get => "/translator/text/batch/v1.0-preview.1/batches"; }
        protected override string Tag { get => "[Azure Translator: Document]"; }

        private readonly ILogger logger;
    }
}
