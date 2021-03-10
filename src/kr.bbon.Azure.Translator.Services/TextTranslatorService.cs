using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using kr.bbon.Azure.Translator.Services.Models;
using kr.bbon.Azure.Translator.Services.Models.TextTranslation.TranslationRequest;

namespace kr.bbon.Azure.Translator.Services
{
    public interface ITextTranslatorService
    {
        Task<IEnumerable<ResponseModel>> TranslateAsync(RequestModel model);
    }

    public class TextTranslatorService : TranslatorServiceBase, ITextTranslatorService
    {
        protected override string Tag { get => "[Azure Translator: Text]"; }
        protected override string Route { get => "/translate?api-version=3.0"; }

        public TextTranslatorService(
            IOptionsMonitor<AzureTranslatorConnectionOptions> azureTranslatorConnectionOptionsAccessor, 
            ILoggerFactory loggerFactory)
            : base(azureTranslatorConnectionOptionsAccessor)
        {
            logger = loggerFactory.CreateLogger<TextTranslatorService>();
        }

        public async Task<IEnumerable<ResponseModel>> TranslateAsync(RequestModel model)
        {
            ValidateAzureTranslateConnectionOptions();
            ValidateRequestbody(model);

            List<ResponseModel> resultSet = null;

            var requestBody = SerializeToJson(model.Inputs);

            using (var client = new HttpClient())
            {
                foreach (var uri in getRequestUri(model))
                {
                    using (var request = new HttpRequestMessage())
                    {
                        request.Method = HttpMethod.Post;
                        request.RequestUri = uri;

                        request.Headers.Add(OCP_APIM_SUBSCRIPTION_KEY, options.SubscriptionKey);
                        request.Headers.Add(OCP_APIM_SUBSCRIPTION_REGION, options.Region);

                        request.Content = new StringContent(requestBody, Encoding.UTF8, CONTENT_TYPE_VALUE);

                        var response = await client.SendAsync(request);

                        if (response.Content == null)
                        {
                            throw new Exception($"{Tag} Response content is empty.");
                        }

                        var resultJson = await response.Content.ReadAsStringAsync();

                        var jsonSerializerOptions = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        };

                        if (response.IsSuccessStatusCode)
                        {
                            var resultModel = JsonSerializer.Deserialize<IEnumerable<Models.TextTranslation.TranslationRequest.ResponseModel>>(resultJson, jsonSerializerOptions);

                            logger.LogInformation($"${Tag} The request has been processed. => Translated.");

                            if (resultSet == null)
                            {
                                resultSet = new List<Models.TextTranslation.TranslationRequest.ResponseModel>(resultModel);
                            }
                            else
                            {
                                var resultModelIndex = 0;
                                resultModel.ToList().ForEach((result) =>
                                {
                                    var list = resultSet[resultModelIndex].Translations.ToList();
                                    result.Translations.ToList().ForEach((translation) =>
                                    {

                                        list.Add(translation);

                                    });

                                    resultSet[resultModelIndex].Translations = list;
                                    resultModelIndex++;
                                });
                            }
                        }
                        else
                        {
                            var resultModel = JsonSerializer.Deserialize<ErrorResponseModel>(resultJson, jsonSerializerOptions);

                            logger.LogInformation($"${Tag} The request does not has been processed. => Not  Translated.");

                            throw new ApiHttpStatusException<ErrorModel<int>>(response.StatusCode, resultModel.Error.Message, resultModel.Error);
                        }
                    }
                }
            }

            return resultSet;
        }

        private IEnumerable<Uri> getRequestUri(RequestModel model)
        {
            if (model.IsTranslationEachLanguage)
            {
                foreach (var language in model.ToLanguages)
                {
                    yield return getRequestUri(model, language);
                }
            }
            else
            {
                yield return getRequestUri(model, string.Empty);
            }
        }

        private Uri getRequestUri(RequestModel model, string languageCode = "")
        {
            var url = GetApiUrl();

            if (string.IsNullOrWhiteSpace(languageCode))
            {
                url = $"{url}&to={String.Join("&to=", model.ToLanguages)}";
            }
            else
            {
                url = $"{url}&to={languageCode}";
            }

            if (!String.IsNullOrWhiteSpace(model.FromLanguage))
            {
                url = $"{url}&from={model.FromLanguage}";
            }

            if (!String.IsNullOrWhiteSpace(model.TextType) && !model.TextType.Equals(TextTypes.Plain, StringComparison.OrdinalIgnoreCase))
            {
                url = $"{url}&textType={model.TextType}";
            }

            if (!String.IsNullOrWhiteSpace(model.Category) && !model.Category.Equals(Categories.General, StringComparison.OrdinalIgnoreCase))
            {
                url = $"{url}&category={model.Category}";
            }

            if (!String.IsNullOrWhiteSpace(model.ProfanityAction) && !model.ProfanityAction.Equals(ProfanityActions.NoAction, StringComparison.OrdinalIgnoreCase))
            {
                url = $"{url}&profanityAction={model.ProfanityAction}";

                if (!String.IsNullOrWhiteSpace(model.ProfanityMarker) && !model.ProfanityMarker.Equals(ProfanityMarkers.Asterisk, StringComparison.OrdinalIgnoreCase))
                {
                    url = $"{url}&profanityMarker={model.ProfanityMarker}";
                }
            }

            var requestUri = new Uri(url);

            //logger.LogInformation($"{TAG} Request uri: {requestUri.ToString()}");

            return requestUri;
        }

        private void ValidateRequestbody(RequestModel model)
        {
            var message = "";
            var errorMessage = new List<string>();

            var inputsCount = model.Inputs.Count();

            if (inputsCount == 0)
            {
                errorMessage.Add("Text to translate is required.");
            }

            if (inputsCount > 100)
            {
                errorMessage.Add("The array can have at most 100 elements.");
            }

            foreach (var input in model.Inputs)
            {
                // https://docs.microsoft.com/en-us/azure/cognitive-services/translator/request-limits#character-and-array-limits-per-request
                // Max 10,000 characters. 
                // Request to translate (+1) and Response to be translated ( + count of to translate languages)
                var contentLength = input.Text.Length * ((model.IsTranslationEachLanguage ? 1 : model.ToLanguages.Count()) + 1);

                logger.LogInformation($"{Tag} Calculated characters={contentLength}");

                if (contentLength > 10000)
                {
                    errorMessage.Add("The entire text included in the request cannot exceed 10,000 characters including spaces.");
                    break;
                }
            }

            if (errorMessage.Count > 0)
            {
                message = "Request body is invalid.";
                logger.LogInformation($"{Tag} {message}");
                throw new ApiHttpStatusException<IEnumerable<string>>(HttpStatusCode.BadRequest, message, errorMessage.ToArray());
            }
        }

        private readonly ILogger logger;
    }
}
