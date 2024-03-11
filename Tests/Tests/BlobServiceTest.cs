using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Azure.Storage;
using Azure.Storage.Blobs;
using TraineeCampTaskReenbit.Services;
using Microsoft.AspNetCore.Http;
using TraineeCampTaskReenbit.DTO;
namespace Tests.Tests
{
    public class BlobServiceTest
    {
        private IConfiguration configuration;
        public BlobServiceTest()
        {
            IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

            this.configuration= configuration;
        }
        [Fact]
        public async Task DownloadAsync_FileExists_ReturnsBlobRequestDTO()
        {
            // Arrange
            var fileName = "example.txt";
            var serviceClientMock = new Mock<BlobServiceClient>(); 
            var containerClientMock = new Mock<BlobContainerClient>();
            var blobClientMock = new Mock<BlobClient>();
            var blobContentMock = new Mock<BlobDownloadInfo>();

            var service = new BlobService(serviceClientMock.Object, configuration); 

            serviceClientMock.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(containerClientMock.Object);
            containerClientMock.Setup(x => x.GetBlobClient(fileName)).Returns(blobClientMock.Object);
            blobClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(true, null));
            blobClientMock.Setup(x => x.OpenReadAsync(It.IsAny<BlobOpenReadOptions>(), default)).ReturnsAsync(new MemoryStream());
            blobClientMock.Setup(_ => _.DownloadContentAsync()).ReturnsAsync(Response.FromValue<BlobDownloadResult>(1, null));
            blobClientMock.Setup(_ => _.DownloadContentAsync()).ReturnsAsync(blobContentMock.Object);
            blobContentMock.Setup(x => x.Details.ContentType).Returns("application/octet-stream");

            // Act
            var result = await service.DownloadAsync(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Name, fileName);
            Assert.NotNull(result.Content);
            Assert.Equal("application/octet-stream", result.ContentType);
        }

        [Fact]
        public async Task DownloadAsync_FileDoesNotExist_ReturnsBlobRequestDTOWithDefaultValues()
        {
            // Arrange
            var fileName = "nonexistent.txt";
            var serviceClientMock = new Mock<BlobServiceClient>();
            var containerClientMock = new Mock<BlobContainerClient>();
            var blobClientMock = new Mock<BlobClient>();

            var service = new BlobService(serviceClientMock.Object, configuration);

            serviceClientMock.Setup(x => x.GetBlobContainerClient(It.IsAny<string>())).Returns(containerClientMock.Object);
            containerClientMock.Setup(x => x.GetBlobClient(fileName)).Returns(blobClientMock.Object);
            blobClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue<bool>(false, null));

            // Act
            var result = await service.DownloadAsync(fileName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(result.Name, fileName);
            Assert.NotNull(result.Content);
            Assert.Null(result.ContentType);
        }
        [Fact]
        public async Task UploadAsync_ValidFile_ReturnsSuccessResponse()
        {
            // Arrange
            var file = new Mock<IFormFile>();
            file.Setup(f => f.FileName).Returns("test.txt");
            file.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
            var email = "test@example.com";

            var serviceClientMock = new Mock<BlobServiceClient>(); // Replace with your actual BlobServiceClient
            var blobService = new BlobService(serviceClientMock.Object,configuration);

            // Act
            var result = await blobService.UploadAsync(file.Object, email);

            // Assert
            Assert.True(result.isSuccess);
            Assert.Contains("test.txt has been uploaded", result.Message);
            Assert.NotNull(result.Blob.URL);
            Assert.NotNull(result.Blob.Name);
        }
        [Fact]
        public async Task GetAllAsync_ReturnsListOfBlobRequestDTO()
        {
            // Arrange
            var mockServiceClient = new Mock<BlobServiceClient>();
            var mockContainerClient = new Mock<BlobContainerClient>();

            var mockBlobItem1 = new Mock<BlobItem>();
            mockBlobItem1.SetupGet(b => b.Name).Returns("file1.txt");

            var mockBlobItem2 = new Mock<BlobItem>();
            mockBlobItem2.SetupGet(b => b.Name).Returns("file2.txt");

            var mockBlobItem3 = new Mock<BlobItem>();
            mockBlobItem3.SetupGet(b => b.Name).Returns("file3.txt");

            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<BlobItem>>();
            mockAsyncEnumerator.Setup(m => m.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockAsyncEnumerator.Setup(m => m.Current).Returns(mockBlobItem1.Object).Verifiable();

            mockContainerClient.Setup(c => c.GetBlobsAsync(It.IsAny<BlobTraits>(), It.IsAny<BlobStates>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockAsyncEnumerator.Object)
                .Verifiable();

            mockServiceClient.Setup(s => s.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(mockContainerClient.Object);

            var blobService = new BlobService(mockServiceClient.Object, configuration);

            // Act
            var result = await blobService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); 
            Assert.All(result, item =>
            {
                Assert.NotNull(item.Name);
                Assert.NotNull(item.URL);
                Assert.NotNull(item.ContentType);
            });

            mockAsyncEnumerator.Verify();
            mockContainerClient.Verify();
            mockServiceClient.Verify();
        }
        [Fact]
        public async Task DeleteAsync_DeleteSuccessfully()
        {
            // Arrange
            var fileName = "testfile.txt";
            var blobContainerName = "testcontainer";
            var mockServiceClient = new Mock<BlobServiceClient>();

            var mockContainerClient = new Mock<BlobContainerClient>();
            mockServiceClient.Setup(x => x.GetBlobContainerClient(blobContainerName)).Returns(mockContainerClient.Object);

            var mockBlobClient = new Mock<BlobClient>();
            mockContainerClient.Setup(x => x.GetBlobClient(fileName)).Returns(mockBlobClient.Object);

            var blobService = new BlobService(mockServiceClient.Object, configuration);

            // Act
            var result = await blobService.DeleteAsync(fileName);

            // Assert
            Assert.True(result.isSuccess);
            Assert.Contains(fileName, result.Message);
            
            mockBlobClient.Verify(x => x.DeleteAsync(It.IsAny <BlobResponseDTO>()), Times.Once);
        }
    }
}