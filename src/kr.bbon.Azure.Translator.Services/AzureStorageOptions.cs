namespace kr.bbon.Azure.Translator.Services
{
    /// <summary>
    /// Represent AzureStorage at appsettings.json
    /// </summary>
    /// <example>Input appsettings.json as below
    /// <code>
    /// {
    ///     "AzureStorage": {
    ///         "ConnectionString": "Your azure blob storage connection string"
    ///     },
    /// }
    /// </code>
    /// </example>
    public class AzureStorageOptions
    {
        public static string Name = "AzureStorage";
        public string ConnectionString { get; set; }
    }

}
