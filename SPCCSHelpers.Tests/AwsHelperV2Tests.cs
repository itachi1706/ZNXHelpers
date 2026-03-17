using System.Security;
using System.Text;
using System.Reflection;
using Amazon;
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

[Collection("EnvironmentVariableDependent")]
public class AwsHelperV2Tests
{
    private readonly AwsHelperV2 _awsHelperV3;
    private readonly Mock<AmazonS3Client> _mockS3Client;
    private readonly Mock<AmazonKeyManagementServiceClient> _mockKmsClient;
    private readonly Mock<AmazonSecretsManagerClient> _mockSecretsManagerClient;
    private readonly Mock<AmazonSimpleSystemsManagementClient> _mockSsmClient;

    public AwsHelperV2Tests()
    {
        Environment.SetEnvironmentVariable("S3_BUCKET_NAME", "testBucket");
        Environment.SetEnvironmentVariable("AWS_SECRET_NAME", "testSecret");
        Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "test");
        Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "test");
        Environment.SetEnvironmentVariable("AWS_PROFILE_NAME", "test");
        _mockS3Client = new Mock<AmazonS3Client>(RegionEndpoint.APSoutheast1);
        _mockKmsClient = new Mock<AmazonKeyManagementServiceClient>(RegionEndpoint.APSoutheast1);
        _mockSecretsManagerClient = new Mock<AmazonSecretsManagerClient>(RegionEndpoint.APSoutheast1);
        _mockSsmClient = new Mock<AmazonSimpleSystemsManagementClient>(RegionEndpoint.APSoutheast1);
        _awsHelperV3 = new AwsHelperV2(_mockS3Client.Object, _mockKmsClient.Object, _mockSecretsManagerClient.Object,
            _mockSsmClient.Object);
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
            }
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
                Plaintext = new MemoryStream(Encoding.UTF8.GetBytes("test"))
            });

        // Act
        var result = await _awsHelperV3.GetSecureStringFromParameterStore(parameterName);

        // Assert
        Assert.Equivalent(secureString, result);
    }

    [Fact]
    public async Task GetStringFromParameterStore_returnsNullWhenParameterIsMissing()
    {
        _mockSsmClient
            .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse { Parameter = null });

        var result = await _awsHelperV3.GetStringFromParameterStore("missingParam");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStringFromParameterStoreSecureString_withDecryptionFalse_returnsKmsDecryptedValue()
    {
        _mockSsmClient
            .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Value = Convert.ToBase64String(Encoding.UTF8.GetBytes("encrypted")),
                    ARN = "arn:aws:ssm:ap-southeast-1:123456789012:parameter/test"
                }
            });

        _mockKmsClient
            .Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DecryptResponse
            {
                Plaintext = new MemoryStream(Encoding.UTF8.GetBytes("decrypted-value"))
            });

        var result = await _awsHelperV3.GetStringFromParameterStoreSecureString("param", false);

        Assert.Equal("decrypted-value", result);
    }

    [Fact]
    public async Task GetStringFromParameterStoreSecureString_throwsWhenCiphertextIsNotBase64()
    {
        _mockSsmClient
            .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse
            {
                Parameter = new Parameter
                {
                    Value = "not-base64",
                    ARN = "arn:aws:ssm:ap-southeast-1:123456789012:parameter/test"
                }
            });

        await Assert.ThrowsAsync<FormatException>(() =>
            _awsHelperV3.GetStringFromParameterStoreSecureString("param", false));
    }

    [Fact]
    public async Task GetSecureStringFromParameterStore_returnsNullWhenParameterIsMissing()
    {
        _mockSsmClient
            .Setup(x => x.GetParameterAsync(It.IsAny<GetParameterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetParameterResponse { Parameter = null });

        var result = await _awsHelperV3.GetSecureStringFromParameterStore("missingParam");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSecretFromSecretsManager_returnsNullWhenResponseIsNull()
    {
        _mockSecretsManagerClient
            .Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetSecretValueResponse)null!);

        var result = await _awsHelperV3.GetSecretFromSecretsManager("missingSecret");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSecretFromSecretsManager_returnsNullWhenIOExceptionOccurs()
    {
        _mockSecretsManagerClient
            .Setup(x => x.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("I/O error"));

        var result = await _awsHelperV3.GetSecretFromSecretsManager("secret");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFileFromS3_withoutBucketParameter_usesConfiguredDefaultBucket()
    {
        GetObjectRequest? capturedRequest = null;
        _mockS3Client
            .Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetObjectRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new GetObjectResponse
            {
                ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes("ok"))
            });

        await _awsHelperV3.GetFileFromS3("file.txt");

        Assert.NotNull(capturedRequest);
        Assert.Equal("testBucket", capturedRequest!.BucketName);
    }

    [Fact]
    public void GetAwsCredentials_throwsWhenProfileIsNotFound()
    {
        var method = typeof(AwsHelperV2).GetMethod("GetAwsCredentials", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var ex = Assert.Throws<TargetInvocationException>(() => method!.Invoke(null, ["profile-does-not-exist"]));

        Assert.IsType<AmazonServiceException>(ex.InnerException);
        Assert.Equal("Failed to get AWS credentials", ex.InnerException!.Message);
    }
}