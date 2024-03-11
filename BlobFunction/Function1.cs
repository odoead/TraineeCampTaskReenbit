using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs;
using System.Net.Mail;
using System.Text;

namespace BlobFunction
{
    public  class Function1
    {
        [FunctionName("Function1")]
        public async Task Run([BlobTrigger("files/{name}")] Stream myBlob, string name,IConfiguration configuration)
        {
            string userEmail = GetUserEmailFromBlobMetadata(name, configuration);
            await SendEmail(userEmail);
        }

        private static string GetUserEmailFromBlobMetadata(string blobName, IConfiguration configuration)
        {
            try
            {
                string blobContainerName = configuration.GetConnectionString("AzureStorage:AzureContainerName");
                string blobStorageconnection = configuration.GetConnectionString("AzureStorageAccount");
                BlobContainerClient ContainerClient = new BlobContainerClient(blobContainerName, blobStorageconnection);
                BlobClient blobClient = ContainerClient.GetBlobClient(blobName);
                BlobProperties properties = blobClient.GetProperties().Value;
                properties.Metadata.TryGetValue("UserEmail", out string email);
                return email;
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception {ex.StackTrace} happened: {ex.Message}");
            }
        }
        private static async Task  SendEmail(string emailTO)
        {
            string emailFrom = "muravjov.kirill@gmail.com";
            MailMessage message = new MailMessage(emailFrom, emailTO);
            string mailbody = "Trainee test email send";
            message.Subject = "Sending Email";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential("id", "1234");
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.SendAsync(message,"Token");
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception {ex.StackTrace} happened: {ex.Message}");
            }

        }
    }

}
