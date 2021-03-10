using System.Text.Encodings.Web;
using System.Text.Json;

namespace kr.bbon.Azure.Translator.Services
{
    public abstract class ServiceBase
    {
        /// <summary>
        /// User defined logger Category 
        /// </summary>
        protected abstract string Tag { get; }

        public string SerializeToJson<T>(T obj, JsonSerializerOptions options = null)
        {
            var actualOptions = options ?? new JsonSerializerOptions
            {
                WriteIndented = true,
                //Encoder = JavaScriptEncoder.Create(UnicodeRanges.All, UnicodeRanges.Cyrillic),
                // ! Caution
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            return JsonSerializer.Serialize<T>(obj, actualOptions);
        }
    }
}
