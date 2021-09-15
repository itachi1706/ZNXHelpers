using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
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
        private readonly string KMSKeyId = EnvHelper.GetString("KMS_KEY_ID");
        private readonly string ProfileName = EnvHelper.GetString("AWS_PROFILE_NAME");
        private readonly string S3BucketName = EnvHelper.GetString("S3_BUCKET_NAME");

        #region AWS Credentials
        /********** CREDENTIALS **********/
        private static AWSCredentials GetAWSCredentials(string profileName)
        {
            var chain = new CredentialProfileStoreChain();
            if (chain.TryGetAWSCredentials(profileName, out AWSCredentials awsCredentials))
            {
                return awsCredentials;
            }
            throw new AmazonServiceException("Failed to get AWS credentials");
        }
        #endregion

        #region AWS Clients
        /********** CLIENTS **********/
        private AmazonKeyManagementServiceClient GetKMSClient()
        {
            return ProfileName == null ?
                new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonKeyManagementServiceClient(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonS3Client GetS3Client()
        {
            return ProfileName == null ?
                new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonS3Client(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSecretsManagerClient GetSecretsManagerClient()
        {
            return ProfileName == null ?
                new AmazonSecretsManagerClient(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonSecretsManagerClient(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSimpleSystemsManagementClient GetSimpleSystemsManagementClient()
        {
            return ProfileName == null ?
                new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.APSoutheast1) :
                new AmazonSimpleSystemsManagementClient(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }
        #endregion

        #region AWS Methods
        /// <summary>
        /// GET string from AWS Parameter Store
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public async Task<string> GetStringFromParameterStore(string parameterName)
        {
            using var ssmClient = GetSimpleSystemsManagementClient();
            var _logger = Log.ForContext<AwsHelperV2>();

            var request = new GetParameterRequest
            {
                Name = parameterName
            };

            var response = await ssmClient.GetParameterAsync(request);

            if (response == null || response.Parameter == null)
            {
                _logger.Error("Unable to get string from parameter store");
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
        public async Task<string> GetStringFromParameterStoreSecureString(string parameterName, bool withDecryption)
        {
            var _logger = Log.ForContext<AwsHelperV2>();
            _logger.Debug("GetStringFromParameterStoreSecureString(" + parameterName + ", " + withDecryption + ")");
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = withDecryption
            };

            var response = await ssmClient.GetParameterAsync(request);

            if (response == null || response.Parameter == null)
            {
                _logger.Error("Unable to get string from parameter store secure string");
                return null;
            }

            if (withDecryption)
            {
                return response.Parameter.Value;
            }

            var encryptedValue = response.Parameter.Value;

            using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));

            var decryptRequest = new DecryptRequest
            {
                KeyId = KMSKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    { "PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            var kmsClient = GetKMSClient();

            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);

            string decryptedString = Encoding.UTF8.GetString(decryptResponse.Plaintext.ToArray());

            return decryptedString;
        }

        /// <summary>
        /// GET secure string from AWS Parameter Store
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public async Task<SecureString> GetSecureStringFromParameterStore(string parameterName)
        {
            var _logger = Log.ForContext<AwsHelperV2>();
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = false
            };

            var response = await ssmClient.GetParameterAsync(request);

            if (response == null || response.Parameter == null)
            {
                _logger.Error("Unable to get secure string from parameter store");
                return null;
            }

            var encryptedValue = response.Parameter.Value;

            using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));

            var decryptRequest = new DecryptRequest
            {
                KeyId = KMSKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    {"PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            var kmsClient = GetKMSClient();

            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);

            SecureString secureString = new SecureString();

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
            return await GetFileFromS3(key, S3BucketName);
        }

        public async Task<byte[]> GetFileFromS3(string key, string bucketName)
        {
            var _logger = Log.ForContext<AwsHelperV2>();
            _logger.Debug("[AwsHelperV2] Inside GetFileFromS3.");
            var s3Client = GetS3Client();

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await s3Client.GetObjectAsync(request);
            using Stream stream = response.ResponseStream;
            using MemoryStream inputStream = new MemoryStream();
            await stream.CopyToAsync(inputStream);
            return inputStream.ToArray();
        }

        /// <summary>
        /// GET secret from AWS Secrets Manager
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetSecretFromSecretsManager(string secretName)
        {
            var _logger = Log.ForContext<AwsHelperV2>();
            _logger.Debug("GetSecretFromSecretsManager(" + secretName + ")");

            var secretsManagerClient = GetSecretsManagerClient();

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            try
            {
                _logger.Debug("Calling secrets manager client");
                var response = await secretsManagerClient.GetSecretValueAsync(request);

                if (response == null)
                {
                    _logger.Error("Unable to get secret from secrets manager");
                    return null;
                }

                _logger.Debug("Deserializing secert");
                _logger.Debug(response.SecretString);
                Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);

                _logger.Debug("Returning secrets");
                return secrets;
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to get secret from secrets manager exception");
                return null;
            }
        }
        #endregion
    }
}
