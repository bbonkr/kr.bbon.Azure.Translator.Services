namespace kr.bbon.Azure.Translator.Services
{
    public interface IAzureBlobStorageContainer
    {
        string GetContainerName();
    }

    public abstract class AzureBlobStorageContainerBase: IAzureBlobStorageContainer
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
