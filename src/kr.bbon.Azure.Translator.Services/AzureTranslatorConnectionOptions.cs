using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kr.bbon.Azure.Translator.Services
{
    public class AzureTranslatorConnectionOptions
    {
        public static string Name = "Translator";

        public string ResourceName { get; set; }

        public string Endpoint { get; set; }

        public string SubscriptionKey { get; set; }

        public string Region { get; set; }
    }
}
