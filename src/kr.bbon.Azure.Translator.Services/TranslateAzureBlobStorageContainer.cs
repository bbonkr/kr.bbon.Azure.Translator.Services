namespace kr.bbon.Azure.Translator.Services
{

    public abstract class AzureBlobStorageContainerBase
    {
        public abstract string GetContainerName();
    }

    public class DocumentTranslationAzureBlobStorageContainer : AzureBlobStorageContainerBase
    {
        public override string GetContainerName()
        {
            return "document-translation";
        }
    }

}
