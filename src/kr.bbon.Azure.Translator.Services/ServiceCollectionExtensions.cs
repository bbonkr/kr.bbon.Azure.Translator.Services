using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using kr.bbon.Azure.Translator.Services;
using kr.bbon.Azure.Translator.Services.Strategies;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configure Azure Translator options
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <example>Input appsettings.json as below
        /// <code>
        /// {
        ///     "Translator": {
        ///         "ResourceName": "Your Azure Translator resource name",
        ///         "Endpoint":"Your Azure Translator endporint",
        ///         "Region": "Your Azure Translator region",
        ///         "SubscriptionKey": "Your Azure Translator subscription key"
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection ConfigureAzureTranslator(this IServiceCollection services, IConfiguration configuration)
        {
        
            services.Configure<AzureTranslatorOptions>(configuration.GetSection(AzureTranslatorOptions.Name));

            return services;
        }

        /// <summary>
        /// Configure Azure Blob Storage options
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <example>Input appsettings.json as below
        /// <code>
        /// {
        ///     "AzureStorage": {
        ///         "ConnectionString": "Your azure blob storage connection string"
        ///     },
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection ConfigureAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureStorageOptions>(configuration.GetSection(AzureStorageOptions.Name));

            return services;
        }

        /// <summary>
        /// Add transient service of <see cref="ITextTranslatorService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAzureTextTranslatorService(this IServiceCollection services)
        {
            services.AddTransient<ITextTranslatorService, TextTranslatorService>();

            return services;
        }

        /// <summary>
        /// Add transient service of <see cref="IStorageService"/> with <see cref="AzureBlobStorageContainerBase"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAzureBlobStorage<TAzureBlobStorageContainerBase>(this IServiceCollection services) where TAzureBlobStorageContainerBase : AzureBlobStorageContainerBase
        {
            services.AddTransient<TAzureBlobStorageContainerBase>();

            services.AddTransient<IStorageService, AzureBlobStorageService>();

            return services;
        }

        /// <summary>
        /// Add transient service of <see cref="IStorageService"/> with <see cref="DocumentTranslationAzureBlobStorageContainer"/>
        /// <para>
        /// Azure BLOB storage container name is `document-translation`.
        /// </para>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDefaultAzureBlobStorage(this IServiceCollection services)
        {
            services.AddAzureBlobStorage<DocumentTranslationAzureBlobStorageContainer>();

            return services;
        }

        /// <summary>
        /// Add transient service of <see cref="IDocumentTranslationService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAzureDocumentTranslatorService(this IServiceCollection services)
        {
            services.AddTransient<IDocumentTranslationService, DocumentTranslationService>();

            return services;
        }

        /// <summary>
        /// Add transient service of <see cref="ITranslatedDocumentNamingStrategy"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAzureDocumentTranslationBlobNamingStrategy(this IServiceCollection services)
        {
            services.AddTransient<ITranslatedDocumentNamingStrategy, TranslatedDocumentNamingStrategy>();

            return services;
        }

        /// <summary>
        /// <para>You will use Text translation and document translation</para>
        /// Configure Azure translation options <see cref="AzureTranslatorOptions"/> and Azure BLOB storage options <see cref="AzureStorageOptions"/>, Add transient of <see cref="ITextTranslatorService"/>, <see cref="IStorageService"/>, <see cref="ITranslatedDocumentNamingStrategy"/>, <see cref="DocumentTranslationAzureBlobStorageContainer"/>, <see cref="IDocumentTranslationService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <example>Input appsettings.json as below
        /// <code>
        /// {
        ///     "AzureStorage": {
        ///         "ConnectionString": "Your azure blob storage connection string"
        ///     },
        ///     "Translator": {
        ///         "ResourceName": "Your Azure Translator resource name",
        ///         "Endpoint":"Your Azure Translator endporint",
        ///         "Region": "Your Azure Translator region",
        ///         "SubscriptionKey": "Your Azure Translator subscription key"
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection AddAzureTranslatorServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureAzureTranslator(configuration)
                .ConfigureAzureBlobStorage(configuration)
                .AddAzureDocumentTranslationBlobNamingStrategy()
                .AddDefaultAzureBlobStorage()
                .AddAzureTextTranslatorService()
                .AddAzureDocumentTranslatorService();

            return services;
        }

        /// <summary>
        /// <para>You will use Text translation only</para>
        /// Configure Azure translation options <see cref="AzureTranslatorOptions"/>, Add transient of <see cref="ITextTranslatorService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <example>Input appsettings.json as below
        /// <code>
        /// {
        ///     "Translator": {
        ///         "ResourceName": "Your Azure Translator resource name",
        ///         "Endpoint":"Your Azure Translator endporint",
        ///         "Region": "Your Azure Translator region",
        ///         "SubscriptionKey": "Your Azure Translator subscription key"
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection AddAzureTextTranslatorServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureAzureTranslator(configuration);

            services.AddAzureTextTranslatorService();

            return services;
        }
    }
}
