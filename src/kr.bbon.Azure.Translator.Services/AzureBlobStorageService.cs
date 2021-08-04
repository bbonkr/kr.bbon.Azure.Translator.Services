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
    public interface IStorageService
    {
        Task<BlobCreateResultModel> FindByNameAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        Task<BlobCreateResultModel> CreateAsync(string containerName, string blobName, Stream stream, string contentType, CancellationToken cancellationToken = default);

        Task<BlobCreateResultModel> CreateAsync(string containerName, string blobName, string contents, string contentType, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default);

        string GenerateBlobSasUri(string containerName, string blobName);

        string GenerateBlobSasUri(string containerName, string blobName, string storedPolicyName = "");

        string GenerateBlobSasUri(string containerName, string blobName, BlobSasPermissions permissions, DateTimeOffset expiresOn, string storedPolicyName = "");

        string GenerateContainerSasUri(string containerName);

        string GenerateContainerSasUri(string containerName, string storedPolicyName = "");

        string GenerateContainerSasUri(string containerName, BlobContainerSasPermissions permissions, DateTimeOffset expiresOn, string storedPolicyName = "");

        string GenerateSourceContainerSasUri(string containerName, DateTimeOffset expiresOn);

        string GenerateTargetContainerSasUri(string containerName, DateTimeOffset expiresOn);

        Task<Stream> LoadBlobAsync(string containerName, string name, CancellationToken cancellationToken = default);
    }

   
    public class AzureBlobStorageService : IStorageService
    {
        public AzureBlobStorageService(
            IOptionsMonitor<AzureStorageOptions> azureStorageOptionsAccessor,
            ILoggerFactory loggerFactory)
        {
            this.options = azureStorageOptionsAccessor.CurrentValue ?? throw new ArgumentException($"Does not configure 'AzureStorage' at appsettings. See AzureBlobStorageOptions information.");
            this.logger = loggerFactory.CreateLogger<AzureBlobStorageService>();
            client = new BlobServiceClient(options.ConnectionString);
        }

        public async Task<BlobCreateResultModel> FindByNameAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
        {
            var message = "";
            try
            {
                var blobContainerClient = GetBlobContainerClient(containerName);

                EnsureContainerCreated(blobContainerClient);

                var blobClient = blobContainerClient.GetBlobClient(blobName);

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

        public string GenerateBlobSasUri(string containerName, string blobName)
        {
            var permissions = BlobSasPermissions.Read | BlobSasPermissions.List | BlobSasPermissions.Write;
            var expiresOn = DateTimeOffset.UtcNow + TimeSpan.FromDays(3);

            var sasUri = GenerateBlobSasUri(containerName, blobName, permissions, expiresOn);

            return sasUri.ToString();
        }

        public string GenerateBlobSasUri(string containerName, string blobName, string storedPolicyName = "")
        {
            var permissions = BlobSasPermissions.Read | BlobSasPermissions.List | BlobSasPermissions.Write;
            var expiresOn = DateTimeOffset.UtcNow + TimeSpan.FromDays(3);

            var sasUri = GenerateBlobSasUri(containerName, blobName, permissions, expiresOn, storedPolicyName);

            return sasUri.ToString();
        }

        public string GenerateBlobSasUri(string containerName, string blobName, BlobSasPermissions permissions, DateTimeOffset expiresOn, string storedPolicyName = "")
        {
            var message = "";
            var containerClient = GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!blobClient.CanGenerateSasUri)
            {
                message = "Could not generate the SAS uri.";
                throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.BadRequest, message, new ErrorModel<int>
                {
                    Code = (int)HttpStatusCode.NotAcceptable,
                    Message = message,
                });
            }

            var builder = new BlobSasBuilder(permissions, expiresOn)
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
            };

            if (!string.IsNullOrWhiteSpace(storedPolicyName))
            {
                builder = new BlobSasBuilder();
                builder.Identifier = storedPolicyName;
            }

            var sasUri = blobClient.GenerateSasUri(builder);

            return sasUri.ToString();
        }

        public string GenerateContainerSasUri(string containerName)
        {
            var permissions = BlobContainerSasPermissions.List | BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Write;
            var expiresOn = DateTimeOffset.UtcNow + TimeSpan.FromDays(3);

            var sasUri = GenerateContainerSasUri(containerName, permissions, expiresOn);

            return sasUri;
        }

        public string GenerateContainerSasUri(string containerName, string storedPolicyName = "")
        {
            var permissions = BlobContainerSasPermissions.List | BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Write;
            var expiresOn = DateTimeOffset.UtcNow + TimeSpan.FromDays(3);

            var sasUri = GenerateContainerSasUri(containerName, permissions, expiresOn, storedPolicyName);

            return sasUri;
        }

        public string GenerateContainerSasUri(string containerName, BlobContainerSasPermissions permissions, DateTimeOffset expiresOn, string storedPolicyName = "")
        {
            var message = "";
            var containerClient = GetBlobContainerClient(containerName);
            if (!containerClient.CanGenerateSasUri)
            {
                message = "Could not generate the SAS uri.";
                throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.BadRequest, message, new ErrorModel<int>
                {
                    Code = (int)HttpStatusCode.NotAcceptable,
                    Message = message,
                });
            }

            var builder = new BlobSasBuilder(permissions, expiresOn)
            {
                BlobContainerName = containerClient.Name,
                Resource = "c",
            };

            if (!string.IsNullOrWhiteSpace(storedPolicyName))
            {
                builder = new BlobSasBuilder();
                builder.Identifier = storedPolicyName;
            }

            var sasUri = containerClient.GenerateSasUri(builder);

            return sasUri.ToString();
        }

        public string GenerateSourceContainerSasUri(string containerName, DateTimeOffset expiresOn)
        {
            var uri = GenerateContainerSasUri(containerName, BlobContainerSasPermissions.List | BlobContainerSasPermissions.Read, expiresOn);

            return uri;
        }

        public string GenerateTargetContainerSasUri(string containerName, DateTimeOffset expiresOn)
        {
            var uri = GenerateContainerSasUri(containerName, BlobContainerSasPermissions.List | BlobContainerSasPermissions.Write, expiresOn);

            return uri;
        }

        public async Task<BlobCreateResultModel> CreateAsync(string containerName, string blobName, Stream stream, string contentType = "", CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = GetBlobContainerClient(containerName);

                EnsureContainerCreated(containerClient);

                var blobClient = containerClient.GetBlobClient(blobName);

                var uploadOptions = new BlobUploadOptions();

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    uploadOptions.HttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                }                

                var result = await blobClient.UploadAsync(stream, uploadOptions, cancellationToken);

                return new BlobCreateResultModel
                {
                    ContainerName = blobClient.BlobContainerName,
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

        public async Task<BlobCreateResultModel> CreateAsync(string containerName, string blobName, string contents, string contentType, CancellationToken cancellationToken = default)
        {
            BlobCreateResultModel result;

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(contents);
                    await writer.FlushAsync();
                    
                    stream.Position = 0;

                    result = await CreateAsync(containerName, blobName, stream, contentType, cancellationToken);

                    writer.Close();
                }                

                stream.Close();
            }

            return result;
        }

        public async Task<bool> DeleteAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
        {
            var message = "";
            try
            {
                var containerClient = GetBlobContainerClient(containerName);

                var isContainerExsits = await containerClient.ExistsAsync(cancellationToken);
                if (!isContainerExsits)
                {
                    message = "Could not find a container.";
                    throw new ApiHttpStatusException<ErrorModel<int>>(HttpStatusCode.NotFound, message, new ErrorModel<int>
                    {
                        Code = (int)HttpStatusCode.NotFound,
                        Message = message,
                    });
                }

                var blobClient = containerClient.GetBlobClient(blobName);

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

        public async Task<Stream> LoadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
        {
            var containerClient = GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync(cancellationToken))
            {
                var result = await blobClient.DownloadAsync(cancellationToken);
                return result.Value.Content;
            }

            return null;
        }

        private void EnsureContainerCreated(BlobContainerClient blobContainerClient)
        {
            blobContainerClient.CreateIfNotExists(PublicAccessType.BlobContainer);
        }

        private BlobContainerClient GetBlobContainerClient(string containerName)
        {
            var blobContainerClient = client.GetBlobContainerClient(containerName);

            return blobContainerClient;
        }

        private readonly AzureStorageOptions options;
        private readonly BlobServiceClient client;
        private readonly ILogger logger;
    }
}
