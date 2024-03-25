using System.Net;
using System.Text;
using Amazon;
using Amazon.KeyManagementService;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Moq;

namespace ZNXHelpers.Tests;

public class AwsHelperV3Tests
{
    private readonly AwsHelperV3 _awsHelperV3;
    private readonly Mock<AmazonS3Client> _mockS3Client;
    private readonly Mock<AmazonKeyManagementServiceClient> _mockKmsClient;
    private readonly Mock<AmazonSecretsManagerClient> _mockSecretsManagerClient;
    private readonly Mock<AmazonSimpleSystemsManagementClient> _mockSsmClient;

    public AwsHelperV3Tests()
    {
        Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "testBucket");
        _mockS3Client = new Mock<AmazonS3Client>(RegionEndpoint.APSoutheast1);
        _mockKmsClient = new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast1);
        _mockSecretsManagerClient = new Mock<AmazonSecretsManagerClient>(RegionEndpoint.APSoutheast1);
        _mockSsmClient = new Mock<AmazonSimpleSystemsManagementClient>(RegionEndpoint.APSoutheast1);
        _awsHelperV3 = new AwsHelperV3(_mockS3Client.Object, _mockKmsClient.Object, _mockSecretsManagerClient.Object, _mockSsmClient.Object);
    }

    [Fact]
    public async Task TestGetFileFromS3()
    {
        // Arrange
        var key = "testKey";
        var bucketName = "testBucket";
        var response = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };
        
        _mockS3Client.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _awsHelperV3.GetFileFromS3(key, bucketName);

        // Assert
        Assert.Equal(Encoding.UTF8.GetBytes("test content"), result);
        
        var response2 = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"))
        };
        _mockS3Client.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response2);
        
        // Act
        var result2 = await _awsHelperV3.GetFileFromS3(key);

        // Assert
        Assert.Equal(Encoding.UTF8.GetBytes("test content"), result2);
    }
    
    [Fact]
    public async Task TestPutFileToS3()
    {
        // Arrange
        var key = "testKey";
        var bucketName = "testBucket";
        var fileContent = Encoding.UTF8.GetBytes("test content");

        var response = new PutObjectResponse()
        {
            HttpStatusCode = HttpStatusCode.OK
        };
        
        var response2 = new PutObjectResponse()
        {
            HttpStatusCode = HttpStatusCode.NoContent
        };
        
        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _awsHelperV3.PutFileToS3(fileContent, key, "text/plain", bucketName);

        // Assert
        Assert.True(result);
        
        result = await _awsHelperV3.PutFileToS3(fileContent, key, "text/plain");

        // Assert
        Assert.True(result);
        
        result = await _awsHelperV3.PutFileToS3(fileContent, key);

        // Assert
        Assert.True(result);
        
        _mockS3Client.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response2);
        
        // Act
        var result2 = await _awsHelperV3.PutFileToS3(fileContent, key, "text/plain", bucketName);

        // Assert
        Assert.False(result2);
    }

    [Fact]
    public void TestGeneratePreSignedS3UrlDownload()
    {
        // Arrange
        var key = "testKey";
        var expiry = 3600;
        var bucketName = "testBucket";

        var response = "https://s3.ap-southeast-1.amazonaws.com/testBucket/testKey";
        
        // Act
        var result = _awsHelperV3.GeneratePreSignedS3UrlDownload(key, expiry, bucketName);
        
        // Assert
        Assert.Contains(response, result);
        
        result = _awsHelperV3.GeneratePreSignedS3UrlDownload(key, expiry);
        
        // Assert
        Assert.Contains(response, result);
    }

    // Add more tests for other methods in the AwsHelperV3 class
}