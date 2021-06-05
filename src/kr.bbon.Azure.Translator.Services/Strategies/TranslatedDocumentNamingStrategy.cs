using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr.bbon.Azure.Translator.Services.Strategies
{
    /// <summary>
    /// Translated document naming strategy
    /// </summary>
    public interface ITranslatedDocumentNamingStrategy
    {
        /// <summary>
        /// Get translated document name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="languageCode"></param>
        /// <returns></returns>
        string GetTranslatedDocumentName(string name, string languageCode);
    }

    /// <summary>
    /// Default translated document naming strategy
    /// </summary>
    public class TranslatedDocumentNamingStrategy: ITranslatedDocumentNamingStrategy
    {
        /// <inheritdoc />
        public string GetTranslatedDocumentName(string name, string languageCode)
        {
            var (fileName, extension) = GetNameToken(name);
            var delimiter = string.IsNullOrWhiteSpace(extension) ? "" : ".";
            
            return $"{fileName}.{languageCode.ToLower()}{delimiter}{extension}";
        }

        private (string FileName, string Extension) GetNameToken(string name)
        {
            var tokens = name.Split('.');

            return (
                FileName: tokens.Length > 1 ? string.Join('.', tokens.Take(tokens.Length - 1)) : tokens.FirstOrDefault(),
                Extension: tokens.Length > 1 ? tokens.Last() : ""
                );
        }
    }
}
