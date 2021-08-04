using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr.bbon.Azure.Translator.Services
{
    /// <summary>
    /// Represent Translator at appsettings.json
    /// </summary>
    /// <example>Input appsettings.json as below
    /// <code>
    /// {
    ///     "Translator": {
    ///         "ResourceName": "Your Azure Translator resource name",
    ///         "Endpoint":"Your Azure Translator endporint",
    ///         "Region": "Your Azure Translator region",
    ///         "SubscriptionKey": "Your Azure Translator subscription key",
    ///         "SourceBlobContainerName": "Source blob container name",
    ///         "TargetBlobContainerName": "Target blob container name"
    ///     }
    /// }
    /// </code>
    /// </example>    
    public class AzureTranslatorOptions
    {
        public static string Name = "Translator";

        public string ResourceName { get; set; }

        public string Endpoint { get; set; }

        public string SubscriptionKey { get; set; }

        public string Region { get; set; }

        public string SourceBlobContainerName { get; set; }

        public string TargetBlobContainerName { get; set; }
    }
}
