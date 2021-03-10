using System;
using System.Collections.Generic;

using Microsoft.Extensions.Options;

namespace kr.bbon.Azure.Translator.Services
{
    public abstract class TranslatorServiceBase: ServiceBase
    {
        protected const string OCP_APIM_SUBSCRIPTION_KEY = "Ocp-Apim-Subscription-Key";
        protected const string OCP_APIM_SUBSCRIPTION_REGION = "Ocp-Apim-Subscription-Region";
        protected const string CONTENT_TYPE_KEY = "Content-Type";
        protected const string CONTENT_TYPE_VALUE = "application/json";

        public TranslatorServiceBase(IOptionsMonitor<AzureTranslatorConnectionOptions> azureTranslatorConnectionOptionsAccessor)
        {
            options = azureTranslatorConnectionOptionsAccessor.CurrentValue;
            errorMessages = new List<string>();
        }

        /// <summary>
        /// Api route
        /// <para>
        /// Route must start a '/'
        /// </para>
        /// </summary>
        protected abstract string Route { get; }      

        protected virtual string GetBaseUrl()
        {
            var endpoint = options.Endpoint;
            if (endpoint.EndsWith("/"))
            {
                endpoint = endpoint.Substring(0, endpoint.Length - 1);
            }

            var url = $"{endpoint}";

            return url;
        }

        /// <summary>
        /// Api base url
        /// </summary>
        /// <returns></returns>
        protected virtual string GetApiUrl()
        {
            var url = $"{GetBaseUrl()}{Route}";

            return url;
        }

        protected virtual void ValidateConditionAzureTranslateConnectionOptions()
        {
            if (string.IsNullOrWhiteSpace(options.Endpoint))
            {
                errorMessages.Add($"{nameof(AzureTranslatorConnectionOptions.Endpoint)} is required");
            }

            if (string.IsNullOrWhiteSpace(options.Region))
            {
                errorMessages.Add($"{nameof(AzureTranslatorConnectionOptions.Region)} is required");
            }

            if (string.IsNullOrWhiteSpace(options.SubscriptionKey))
            {
                errorMessages.Add($"{nameof(AzureTranslatorConnectionOptions.SubscriptionKey)} is required");
            }
        }

        protected void ValidateAzureTranslateConnectionOptions()
        {
            errorMessages.Clear();

            ValidateConditionAzureTranslateConnectionOptions();

            if (errorMessages.Count > 0)
            {
                throw new OptionsValidationException(AzureTranslatorConnectionOptions.Name, typeof(AzureTranslatorConnectionOptions), errorMessages);
            }
        }

        protected readonly AzureTranslatorConnectionOptions options;
        protected readonly IList<string> errorMessages;
    }
}
