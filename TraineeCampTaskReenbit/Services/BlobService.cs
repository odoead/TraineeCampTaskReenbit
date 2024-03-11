using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System.CodeDom.Compiler;
using TraineeCampTaskReenbit.DTO;

namespace TraineeCampTaskReenbit.Services
{
    public class BlobService:IBlobService
    {
        private BlobServiceClient serviceClient;
        private string blobContainerName;
        private IConfiguration configuration;
        public BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            serviceClient = blobServiceClient;
            this.configuration = configuration;
            blobContainerName = configuration.GetConnectionString("AzureStorage:AzureContainerName");

        }

        public async Task<List<BlobRequestDTO>> GetAllAsync()
        {
            List<BlobRequestDTO> files = new List<BlobRequestDTO>();
            try
            {
                BlobContainerClient container = serviceClient.GetBlobContainerClient(blobContainerName);
                await foreach (var file in container.GetBlobsAsync())
                {
                    string url = container.Uri.ToString();
                    var name = file.Name;
                    BlobRequestDTO blob = new BlobRequestDTO();
                    blob.Name = name;
                    blob.URL = $"{url}/{name}";
                    blob.ContentType = file.Properties.ContentType;
                    files.Add(blob);
                }
            }
            catch (Exception ex)
            { throw new Exception($"Exception {ex.StackTrace} happened: {ex.Message}"); }
            return files;
        }
        public async Task<BlobResponseDTO> UploadAsync(IFormFile file, string Email)
        {
            BlobResponseDTO response = new BlobResponseDTO();

            try
            {
                BlobContainerClient container = serviceClient.GetBlobContainerClient(blobContainerName);
                await container.CreateIfNotExistsAsync();
                BlobClient client = container.GetBlobClient(file.FileName);

                await using (Stream? data = file.OpenReadStream())
                {
                    await client.UploadAsync(data);
                }

                var SAStoken = GeneratedSASToken(file.FileName);
                var sasUrl = client.Uri.AbsoluteUri + "?" + SAStoken;
                await client.SetMetadataAsync(new Dictionary<string, string> { { "Email", Email } });

                response.Message = $"{file.FileName} has been uploaded";
                response.isSuccess = true;
                response.Blob.URL = sasUrl;
                response.Blob.Name = client.Name;
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Exception {ex.StackTrace} happened: {ex.Message}";
                return response;
            }

            return response;
        }
        private string GeneratedSASToken(string fileName)
        {
            var azureStorageAccount = configuration.GetSection("AzureStorage:AzureAccount").Value;
            var azureStorageAccessKey = configuration.GetSection("AzureStorage:AccessKey").Value;
            BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobContainerName,
                BlobName = fileName,
                ExpiresOn = DateTime.UtcNow.AddHours(1),
            };
            blobSasBuilder.SetPermissions(BlobSasPermissions.All);
            var sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(azureStorageAccount,
               azureStorageAccessKey)).ToString();
            return sasToken;
        }
        public async Task<BlobResponseDTO> DeleteAsync(string FileName)
        {
            BlobResponseDTO response = new BlobResponseDTO();
            BlobContainerClient container = serviceClient.GetBlobContainerClient(blobContainerName);
            BlobClient file = container.GetBlobClient(FileName);
            try
            {
                await file.DeleteAsync();
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Exception {ex.StackTrace} happened: {ex.Message}";
                return response;
            }
            response.isSuccess = true;
            response.Message = $"{FileName} has been deleted";
            return response;
        }
        public async Task<BlobRequestDTO> DownloadAsync(string FileName)
        {
            BlobRequestDTO Blob = new BlobRequestDTO();
            BlobContainerClient container = serviceClient.GetBlobContainerClient(blobContainerName);
            BlobClient file = container.GetBlobClient(FileName);
            try
            {
                if (await file.ExistsAsync())
                {
                    var readStream = await file.OpenReadAsync();
                    var content = await file.DownloadContentAsync();

                    Blob.Content = readStream.ToString();
                    Blob.Name = FileName;
                    Blob.ContentType = content.Value.Details.ContentType;
                    return Blob;
                }
            }
            catch
            (Exception ex)
            {
                throw new Exception($"Exception {ex.StackTrace} happened: {ex.Message}");
            }
            
            return Blob;
        }

        
    }
}

