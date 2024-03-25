using System.Security;
using System.Text;
using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Newtonsoft.Json;
using Serilog;

namespace ZNXHelpers
{
    public class AwsHelperV2
    {
        private readonly string? _kmsKeyId = EnvHelper.GetString("KMS_KEY_ID");
        private readonly string? _profileName = EnvHelper.GetString("AWS_PROFILE_NAME");
        private readonly string? _s3BucketName = EnvHelper.GetString("S3_BUCKET_NAME");

        #region AWS Credentials
        /********** CREDENTIALS **********/
        private static AWSCredentials GetAwsCredentials(string? profileName)
        {
            var chain = new CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials(profileName, out var awsCredentials))
            {
                return awsCredentials;
            }
            throw new AmazonServiceException("Failed to get AWS credentials");
        }
        #endregion

        #region AWS Clients
        /********** CLIENTS **********/
        private AmazonKeyManagementServiceClient GetKmsClient()
        {
            return _profileName == null ?
                new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonKeyManagementServiceClient(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonS3Client GetS3Client()
        {
            return _profileName == null ?
                new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonS3Client(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSecretsManagerClient GetSecretsManagerClient()
        {
            return _profileName == null ?
                new AmazonSecretsManagerClient(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonSecretsManagerClient(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSimpleSystemsManagementClient GetSimpleSystemsManagementClient()
        {
            return _profileName == null ?
                new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonSimpleSystemsManagementClient(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }
        #endregion

        #region AWS Methods
        /// <summary>
        /// GET string from AWS Parameter Store
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public async Task<string?> GetStringFromParameterStore(string parameterName)
        {
            using var ssmClient = GetSimpleSystemsManagementClient();
            var logger = Log.ForContext<AwsHelperV2>();

            var request = new GetParameterRequest
            {
                Name = parameterName
            };

            var response = await ssmClient.GetParameterAsync(request);

            if (response == null || response.Parameter == null)
            {
                logger.Error("Unable to get string from parameter store");
                return null;
            }

            return response.Parameter.Value;
        }

        /// <summary>
        /// GET secure string from AWS Parameter Store and CONVERT to a string type
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="withDecryption"></param>
        /// <returns></returns>
        public async Task<string?> GetStringFromParameterStoreSecureString(string parameterName, bool withDecryption)
        {
            var logger = Log.ForContext<AwsHelperV2>();
            logger.Debug("GetStringFromParameterStoreSecureString({ParameterName}, {WithDecryption})", parameterName, withDecryption);
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = withDecryption
            };

            var response = await ssmClient.GetParameterAsync(request);

            if (response == null || response.Parameter == null)
            {
                logger.Error("Unable to get string from parameter store secure string");
                return null;
            }

            if (withDecryption)
            {
                return response.Parameter.Value;
            }

            var encryptedValue = response.Parameter.Value;

            using var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));

            var decryptRequest = new DecryptRequest
            {
                KeyId = _kmsKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    { "PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            var kmsClient = GetKmsClient();

            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);

            var decryptedString = Encoding.UTF8.GetString(decryptResponse.Plaintext.ToArray());

            return decryptedString;
        }

        /// <summary>
        /// GET secure string from AWS Parameter Store
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public async Task<SecureString?> GetSecureStringFromParameterStore(string parameterName)
        {
            var logger = Log.ForContext<AwsHelperV2>();
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = false
            };

            var response = await ssmClient.GetParameterAsync(request);

            if (response == null || response.Parameter == null)
            {
                logger.Error("Unable to get secure string from parameter store");
                return null;
            }

            var encryptedValue = response.Parameter.Value;

            using var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));

            var decryptRequest = new DecryptRequest
            {
                KeyId = _kmsKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    {"PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            var kmsClient = GetKmsClient();

            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);

            var secureString = new SecureString();

            using var reader = new StreamReader(decryptResponse.Plaintext);

            while (reader.Peek() >= 0)
            {
                secureString.AppendChar((char)reader.Read());
            }

            return secureString;
        }

        /// <summary>
        /// GET file from AWS S3
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFileFromS3(string key)
        {
            return await GetFileFromS3(key, _s3BucketName);
        }

        public async Task<byte[]> GetFileFromS3(string key, string? bucketName)
        {
            var logger = Log.ForContext<AwsHelperV2>();
            logger.Debug("[AwsHelperV2] Inside GetFileFromS3");
            var s3Client = GetS3Client();

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await s3Client.GetObjectAsync(request);
            using var stream = response.ResponseStream;
            using var inputStream = new MemoryStream();
            await stream.CopyToAsync(inputStream);
            return inputStream.ToArray();
        }

        /// <summary>
        /// GET secret from AWS Secrets Manager
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>?> GetSecretFromSecretsManager(string secretName)
        {
            var logger = Log.ForContext<AwsHelperV2>();
            logger.Debug("GetSecretFromSecretsManager({SecretName})", secretName);

            var secretsManagerClient = GetSecretsManagerClient();

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            try
            {
                logger.Debug("Calling secrets manager client");
                var response = await secretsManagerClient.GetSecretValueAsync(request);

                if (response == null)
                {
                    logger.Error("Unable to get secret from secrets manager");
                    return null;
                }

                logger.Debug("Deserializing secret");
                logger.Debug("{SS}", response.SecretString);
                var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);

                logger.Debug("Returning secrets");
                return secrets;
            }
            catch (IOException ex)
            {
                logger.Error(ex, "Unable to get secret from secrets manager exception");
                return null;
            }
        }
        #endregion
    }
}
