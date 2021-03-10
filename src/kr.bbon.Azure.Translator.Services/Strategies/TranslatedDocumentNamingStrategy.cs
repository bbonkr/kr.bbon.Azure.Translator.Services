using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr.bbon.Azure.Translator.Services.Strategies
{
    public interface ITranslatedDocumentNamingStrategy
    {
        string GetTranslatedDocumentName(string name, string languageCode);
    }

    public class TranslatedDocumentNamingStrategy: ITranslatedDocumentNamingStrategy
    {
        public string GetTranslatedDocumentName(string name, string languageCode)
        {
            var (fileName, extension) = GetNameToken(name);
            var delimiter = string.IsNullOrWhiteSpace(extension) ? "" : ".";
            
            return $"{fileName}{delimiter}{extension}";
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
