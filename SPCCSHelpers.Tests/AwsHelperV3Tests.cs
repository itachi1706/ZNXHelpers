using System.Net;
using System.Security;
using System.Text;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Moq;

namespace SPCCSHelpers.Tests;

public class AwsHelperV3Tests
{
    private readonly AwsHelperV3 _awsHelperV3;
    private readonly Mock<AmazonS3Client> _mockS3Client;
    private readonly Mock<AmazonKeyManagementServiceClient> _mockKmsClient;
    private readonly Mock<AmazonSecretsManagerClient> _mockSecretsManagerClient;
    private readonly Mock<AmazonSimpleSystemsManagementClient> _mockSsmClient;
    private readonly Mock<AmazonCloudWatchClient> _mockCwClient;

    public AwsHelperV3Tests()
    {
        Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "testBucket");
        Environment.SetEnvironmentVariable("AWS_SECRET_NAME", "testSecret");
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test");
        Environment.SetEnvironmentVariable("AWS_PRINT_STACK_TRACE", "true");
        Environment.SetEnvironmentVariable("AWS_BASIC_AUTH", "true");
        Environment.SetEnvironmentVariable("AWS_VERBOSE_DEBUG", "true");
        Environment.SetEnvironmentVariable("AWS_REQUEST_ID_DEBUG", "true");
        Environment.SetEnvironmentVariable("AWS_RESPONSE_METADATA_DEBUG", "true");
        _mockS3Client = new Mock<AmazonS3Client>(RegionEndpoint.APSoutheast1);
        _mockKmsClient = new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast1);
        _mockSecretsManagerClient = new Mock<AmazonSecretsManagerClient>(RegionEndpoint.APSoutheast1);
        _mockSsmClient = new Mock<AmazonSimpleSystemsManagementClient>(RegionEndpoint.APSoutheast1);
        _mockCwClient = new Mock<AmazonCloudWatchClient>(RegionEndpoint.APSoutheast1);
        _awsHelperV3 = new AwsHelperV3(_mockS3Client.Object, _mockKmsClient.Object, _mockSecretsManagerClient.Object, _mockSsmClient.Object, _mockCwClient.Object);
    }

    [Fact]
    public async Task TestGetFileFromS3()
    {
        // Arrange
        var key = "testKey";
        var bucketName = "testBucket";
        var response = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("test content")),
            ResponseMetadata = new ResponseMetadata()
        };
        
        _mockS3Client.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _awsHelperV3.GetFileFromS3(key, bucketName);

        // Assert
        Assert.Equal(Encoding.UTF8.GetBytes("test content"), result);
        
        var response2 = new GetObjectResponse
        {
            ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("test content")),
            ResponseMetadata = new ResponseMetadata()
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
            HttpStatusCode = HttpStatusCode.OK,
            ResponseMetadata = new ResponseMetadata()
        };
        
        var response2 = new PutObjectResponse()
        {
            HttpStatusCode = HttpStatusCode.NoContent,
            ResponseMetadata = new ResponseMetadata()
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

    [Fact]
    public async Task TestGetSecretFromSecretManager()
    {
        var secretName = "test";
        var response = new GetSecretValueResponse()
        {
            SecretString = "{\"name\":\"test\"}",
            ResponseMetadata = new ResponseMetadata()
        };
        
        
        _mockSecretsManagerClient.Setup(x =>
                x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        // Act
        var result = await _awsHelperV3.GetSecretFromSecretsManager(secretName);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result["name"]);
        
        result = await _awsHelperV3.GetSecretFromSecretsManager();
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result["name"]);
    }

    [Fact]
    public async Task TestGetStringFromParameterStore()
    {
        var parameterName = "testParam";
        var response = new GetParameterResponse()
        {
            Parameter = new Parameter()
            {
                Value = "test"
            },
            ResponseMetadata = new ResponseMetadata()
        };
        
        _mockSsmClient.Setup(x =>
                x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        // Act
        var result = await _awsHelperV3.GetStringFromParameterStore(parameterName);
        
        // Assert
        Assert.Equal("test", result);
        
        result = await _awsHelperV3.GetStringFromParameterStoreSecureString(parameterName, true);
        
        // Assert
        Assert.Equal("test", result);
    }
    
    [Fact]
    public async Task TestGetSecureStringFromParameterStore()
    {
        var parameterName = "testParam";
        var response = new GetParameterResponse()
        {
            Parameter = new Parameter()
            {
                Value = "test"
            },
            ResponseMetadata = new ResponseMetadata()
        };
        
        var secureString = new SecureString();
        secureString.AppendChar('t');
        secureString.AppendChar('e');
        secureString.AppendChar('s');
        secureString.AppendChar('t');
        
        _mockSsmClient.Setup(x =>
                x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        _mockKmsClient.Setup(x =>
                x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecryptResponse()
            {
                Plaintext = new MemoryStream(Encoding.UTF8.GetBytes("test")),
                ResponseMetadata = new ResponseMetadata()
            });
        
        // Act
        var result = await _awsHelperV3.GetSecureStringFromParameterStore(parameterName);
        
        // Assert
        Assert.Equivalent(secureString, result);
    }

    [Fact]
    public async Task PushMetric_withClient_skipsPublishingWhenCustomMetricsDisabled()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "false");

        var helper = new AwsHelperV3(
            _mockS3Client.Object,
            _mockKmsClient.Object,
            _mockSecretsManagerClient.Object,
            _mockSsmClient.Object,
            _mockCwClient.Object);

        var metricList = new List<MetricDatum>
        {
            new() { MetricName = "RequestCount", Unit = StandardUnit.Count, Value = 1 }
        };

        await helper.PushMetric(_mockCwClient.Object, metricList);

        _mockCwClient.Verify(
            x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PushMetric_withClient_usesDefaultNamespaceWhenNamespaceNotProvided()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "true");
        Environment.SetEnvironmentVariable("METRICS_NAMESPACE", "App/UnitTests");

        var helper = new AwsHelperV3(
            _mockS3Client.Object,
            _mockKmsClient.Object,
            _mockSecretsManagerClient.Object,
            _mockSsmClient.Object,
            _mockCwClient.Object);

        PutMetricDataRequest? capturedRequest = null;
        _mockCwClient
            .Setup(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutMetricDataRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutMetricDataResponse { ResponseMetadata = new ResponseMetadata() });

        var metricList = new List<MetricDatum>
        {
            new() { MetricName = "Latency", Unit = StandardUnit.Milliseconds, Value = 25 }
        };

        await helper.PushMetric(_mockCwClient.Object, metricList);

        Assert.NotNull(capturedRequest);
        Assert.Equal("App/UnitTests", capturedRequest!.Namespace);
        Assert.Single(capturedRequest.MetricData);
    }

    [Fact]
    public async Task PushMetric_byNameAndDimensions_defaultsUnitToCount()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "true");
        Environment.SetEnvironmentVariable("METRICS_NAMESPACE", "App/UnitTests");

        var helper = new AwsHelperV3(
            _mockS3Client.Object,
            _mockKmsClient.Object,
            _mockSecretsManagerClient.Object,
            _mockSsmClient.Object,
            _mockCwClient.Object);

        PutMetricDataRequest? capturedRequest = null;
        _mockCwClient
            .Setup(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutMetricDataRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutMetricDataResponse { ResponseMetadata = new ResponseMetadata() });

        var dimensions = new List<Dimension>
        {
            new() { Name = "Route", Value = "/health" }
        };

        await helper.PushMetric("HealthCheckCount", 3, dimensions, "App/Custom");

        Assert.NotNull(capturedRequest);
        Assert.Equal("App/Custom", capturedRequest!.Namespace);
        Assert.Single(capturedRequest.MetricData);
        Assert.Equal(StandardUnit.Count, capturedRequest.MetricData[0].Unit);
        Assert.Equal("HealthCheckCount", capturedRequest.MetricData[0].MetricName);
        Assert.Equal(3, capturedRequest.MetricData[0].Value);
        Assert.Equal("Route", capturedRequest.MetricData[0].Dimensions[0].Name);
        Assert.Equal("/health", capturedRequest.MetricData[0].Dimensions[0].Value);
    }

    [Fact]
    public async Task PushMetric_byName_usesAppIdAsUniqueIdentifierWhenNotProvided()
    {
        Environment.SetEnvironmentVariable("AWS_CUSTOM_METRICS", "true");
        Environment.SetEnvironmentVariable("APP_ID", "instance-123");

        var helper = new AwsHelperV3(
            _mockS3Client.Object,
            _mockKmsClient.Object,
            _mockSecretsManagerClient.Object,
            _mockSsmClient.Object,
            _mockCwClient.Object);

        PutMetricDataRequest? capturedRequest = null;
        _mockCwClient
            .Setup(x => x.PutMetricDataAsync(It.IsAny<PutMetricDataRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutMetricDataRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutMetricDataResponse { ResponseMetadata = new ResponseMetadata() });

        await helper.PushMetric("WorkerHeartbeat", 1);

        Assert.NotNull(capturedRequest);
        Assert.Single(capturedRequest!.MetricData);
        Assert.Single(capturedRequest.MetricData[0].Dimensions);
        Assert.Equal("UniqueIdentifier", capturedRequest.MetricData[0].Dimensions[0].Name);
        Assert.Equal("instance-123", capturedRequest.MetricData[0].Dimensions[0].Value);
    }
}