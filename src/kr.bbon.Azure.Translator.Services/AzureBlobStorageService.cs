using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using kr.bbon.Azure.Translator.Services.Models;
using kr.bbon.Azure.Translator.Services.Models.AzureStorage.Blob;

namespace kr.bbon.Azure.Translator.Services
{
    public interface IStorageService<T>
    {
        Task<BlobCreateResultModel> FindByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<BlobCreateResultModel> CreateAsync(string name, Stream stream, string contentType, CancellationToken cancellationToken = default);

        Task<BlobCreateResultModel> CreateAsync(string name, string contents, string contentType, CancellationToken cancellationToken = default);


        Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default);

        string GenerateSasUri(string name, string storedPolicyName = "");
    }

   
    public class AzureBlobStorageService<T> : IStorageService<T> where T: AzureBlobStorageContainerBase
    {
        public AzureBlobStorageService(IOptionsMonitor<AzureStorageOptions> azureStorageOptionsAccessor, ILoggerFactory loggerFactory)
        {
            this.options = azureStorageOptionsAccessor.CurrentValue;
            this.logger = loggerFactory.CreateLogger<AzureBlobStorageService<T>>();

            var container = Activator.CreateInstance<T>();
            this.client = new BlobContainerClient(options.ConnectionString, container.GetContainerName());
        }

        public async Task<BlobCreateResultModel> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var message = "";
            try
            {
                EnsureContainerCreated();

                var blobClient = client.GetBlobClient(name);

                var exists = await blobClient.ExistsAsync(cancellationToken);

                if (exists)
                {
                    var result = new BlobCreateResultModel
                    {
                        BlobName = blobClient.Name,
                        Uri = blobClient.Uri.ToString(),
                        ContainerName = blobClient.BlobContainerName,
                    };

                    return result;
                }
                else
                {
                    message = "Could not find a file.";
                    var details = new ErrorModel<int>
                    {
                        Code = (int)HttpStatusCode.NotFound,
                        Message = message,
                    };
                    throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.NotFound, message, details);
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                throw;
            }
        }

        public string GenerateSasUri(string name, string storedPolicyName = "")
        {
            var message = "";
            var blobClient = client.GetBlobClient(name);

            if (!client.CanGenerateSasUri || !blobClient.CanGenerateSasUri)
            {
                message = "Could not generate the SAS uri.";
                throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.BadRequest, message, new ErrorModel<int>
                {
                    Code = (int)HttpStatusCode.NotAcceptable,
                    Message = message,
                });
            }

            var builder = new BlobSasBuilder()
            {
                BlobContainerName = client.Name,
                BlobName = name,
                Resource = "b",
            };


            if (string.IsNullOrWhiteSpace(storedPolicyName))
            {
                builder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(1);
                builder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
            }
            else
            {
                builder.Identifier = storedPolicyName;
            }

            var sasUri = blobClient.GenerateSasUri(builder);

            return sasUri.ToString();
        }

        public async Task<BlobCreateResultModel> CreateAsync(string name, Stream stream, string contentType = "", CancellationToken cancellationToken = default)
        {
            try
            {
                EnsureContainerCreated();

                var blobClient = client.GetBlobClient(name);

                var uploadOptions = new BlobUploadOptions();

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    uploadOptions.HttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                }                

                var result = await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

                return new BlobCreateResultModel
                {
                    ContainerName = client.Name,
                    BlobName = blobClient.Name,
                    Uri = blobClient.Uri.ToString(),
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                throw;
            }
        }

        public async Task<BlobCreateResultModel> CreateAsync(string name, string contents, string contentType, CancellationToken cancellationToken = default)
        {
            BlobCreateResultModel result;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(contents);
                    await writer.FlushAsync();
                    writer.Close();
                }

                result = await CreateAsync(name, stream, contentType, cancellationToken);

                stream.Close();
            }

            return result;
        }

        public async Task<bool> DeleteAsync(string name, CancellationToken cancellationToken = default)
        {
            var message = "";
            try
            {
                EnsureContainerCreated();

                var blobClient = client.GetBlobClient(name);

                var exists = await blobClient.ExistsAsync(cancellationToken);

                if (exists)
                {
                    var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

                    return result;
                }
                else
                {
                    message = "Could not find a file.";
                    throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.NotFound, message, new ErrorModel<int>
                    {
                        Code = (int)HttpStatusCode.NotFound,
                        Message = message,
                    });
                }
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);

                throw;
            }
        }

        private void EnsureContainerCreated()
        {
            client.CreateIfNotExists(PublicAccessType.BlobContainer);
        }

        private readonly AzureStorageOptions options;
        private readonly BlobContainerClient client;
        private readonly ILogger logger;
    }



}
