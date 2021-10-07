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
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Newtonsoft.Json;
using Serilog;

namespace ZNXHelpers
{
    public class AwsHelperV3
    {
        private readonly string KMSKeyId = EnvHelper.GetString("KMS_KEY_ID");
        private readonly string ProfileName = EnvHelper.GetString("AWS_PROFILE_NAME");
        private readonly string S3BucketName = EnvHelper.GetString("S3_BUCKET_NAME");
        private readonly bool IsAwsEksSa = EnvHelper.GetBool("AWS_EKS_SA", false);
        private readonly bool VerboseLogEnabled = EnvHelper.GetBool("AWS_VERBOSE_DEBUG", false);
        private readonly ILogger _logger;

        public AwsHelperV3()
        {
            _logger = Log.ForContext<AwsHelperV3>();
        }

        #region Utils
        private void VerboseLog(string log)
        {
            if (VerboseLogEnabled)
            {
                _logger.Debug(log);
            }

        }
        #endregion

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

        #region STS
        private async Task<AWSCredentials> GetAwsCredentialsSts()
        {
            IAmazonSecurityTokenService stsClient = new AmazonSecurityTokenServiceClient();
            AWSCredentials stsUser = new Credentials();
            using (var client = stsClient)
            {
                GetSessionTokenRequest getSessionTokenRequest = new GetSessionTokenRequest() { DurationSeconds = 900 };
                VerboseLog("[GetAwsCredentialsSts] Getting STS Session Token");
                GetSessionTokenResponse token = await client.GetSessionTokenAsync();
                VerboseLog("[GetAwsCredentialsSts] Obtained STS Session Token");
                stsUser = token.Credentials;
            }
            VerboseLog("[GetAwsCredentialsSts] Returning STS User");
            return stsUser;
        }
        #endregion

        #region AWS Clients
        /********** CLIENTS **********/
        private AmazonKeyManagementServiceClient GetKMSClient()
        {
            return ProfileName == null ?
                GetKmsClientProd() :
                new AmazonKeyManagementServiceClient(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonKeyManagementServiceClient GetKmsClientProd()
        {
            if (!IsAwsEksSa)
            {
                VerboseLog("[GetKmsClientProd] Returning normal prod client");
                return new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetKmsClientProd] Getting STS credentials");
            var user = GetAwsCredentialsSts();
            user.Wait();
            VerboseLog("[GetKmsClientProd] Returning STS prod client");
            return new AmazonKeyManagementServiceClient(user.Result);
        }

        private AmazonS3Client GetS3Client()
        {
            return ProfileName == null ?
                GetS3ClientProd() :
                new AmazonS3Client(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonS3Client GetS3ClientProd()
        {
            if (!IsAwsEksSa)
            {
                VerboseLog("[GetS3ClientProd] Returning normal prod client");
                return new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetS3ClientProd] Getting STS credentials");
            var user = GetAwsCredentialsSts();
            user.Wait();
            VerboseLog("[GetS3ClientProd] Returning STS prod client");
            return new AmazonS3Client(user.Result);
        }

        private AmazonSecretsManagerClient GetSecretsManagerClient()
        {
            return ProfileName == null ?
                GetSecretsManagerClientProd() :
                new AmazonSecretsManagerClient(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSecretsManagerClient GetSecretsManagerClientProd()
        {
            if (!IsAwsEksSa)
            {
                VerboseLog("[GetSecretsManagerClientProd] Returning normal prod client");
                return new AmazonSecretsManagerClient(Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetSecretsManagerClientProd] Getting STS credentials");
            var user = GetAwsCredentialsSts();
            user.Wait();
            VerboseLog("[GetSecretsManagerClientProd] Returning STS prod client");
            return new AmazonSecretsManagerClient(user.Result);
        }

        private AmazonSimpleSystemsManagementClient GetSimpleSystemsManagementClient()
        {
            return ProfileName == null ?
                GetSimpleSystemsManagementClientProd() :
                new AmazonSimpleSystemsManagementClient(GetAWSCredentials(ProfileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSimpleSystemsManagementClient GetSimpleSystemsManagementClientProd()
        {
            if (!IsAwsEksSa)
            {
                VerboseLog("[GetSimpleSystemsManagementClientProd] Returning normal prod client");
                return new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetSimpleSystemsManagementClientProd] Getting STS credentials");
            var user = GetAwsCredentialsSts();
            user.Wait();
            VerboseLog("[GetSimpleSystemsManagementClientProd] Returning STS prod client");
            return new AmazonSimpleSystemsManagementClient(user.Result);
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

            var request = new GetParameterRequest
            {
                Name = parameterName
            };

            VerboseLog("[GetStringFromParameterStore] Getting String from Parameter Store");
            var response = await ssmClient.GetParameterAsync(request);
            VerboseLog("[GetStringFromParameterStore] Obtained String from Parameter Store");

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
            _logger.Debug("GetStringFromParameterStoreSecureString(" + parameterName + ", " + withDecryption + ")");
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = withDecryption
            };

            VerboseLog("[GetStringFromParameterStoreSecureString] Getting Secure String from Parameter Store");
            var response = await ssmClient.GetParameterAsync(request);
            VerboseLog("[GetStringFromParameterStoreSecureString] Obtained Secure String from Parameter Store");

            if (response == null || response.Parameter == null)
            {
                _logger.Error("Unable to get string from parameter store secure string");
                return null;
            }

            if (withDecryption)
            {
                VerboseLog("[GetStringFromParameterStoreSecureString] Returned String that is decrypted");
                return response.Parameter.Value;
            }
            VerboseLog("[GetStringFromParameterStoreSecureString] Decrypting Secure String");

            var encryptedValue = response.Parameter.Value;

            using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));
            VerboseLog("[GetStringFromParameterStoreSecureString] Generated SecureString Stream");

            var decryptRequest = new DecryptRequest
            {
                KeyId = KMSKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    { "PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            VerboseLog("[GetStringFromParameterStoreSecureString] Getting KMS Client");
            var kmsClient = GetKMSClient();

            VerboseLog("[GetStringFromParameterStoreSecureString] Prepare to decrypt with KMS Client");
            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);
            VerboseLog("[GetStringFromParameterStoreSecureString] Decrypted String with KMS Client");

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
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = false
            };

            VerboseLog("[GetSecureStringFromParameterStore] Getting Secure String from Parameter Store");
            var response = await ssmClient.GetParameterAsync(request);
            VerboseLog("[GetSecureStringFromParameterStore] Obtained Secure String from Parameter Store");

            if (response == null || response.Parameter == null)
            {
                _logger.Error("Unable to get secure string from parameter store");
                return null;
            }

            var encryptedValue = response.Parameter.Value;

            using MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));
            VerboseLog("[GetSecureStringFromParameterStore] Generated SecureString Stream");

            var decryptRequest = new DecryptRequest
            {
                KeyId = KMSKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    {"PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            VerboseLog("[GetSecureStringFromParameterStore] Getting KMS Client");
            var kmsClient = GetKMSClient();

            VerboseLog("[GetSecureStringFromParameterStore] Preparing to decrypt string with KMS Client");
            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);
            VerboseLog("[GetSecureStringFromParameterStore] Decrypted String with KMS Client");

            SecureString secureString = new SecureString();

            using var reader = new StreamReader(decryptResponse.Plaintext);
            VerboseLog("[GetSecureStringFromParameterStore] Converted decrypted string to a stream for insertion to SecureString");

            while (reader.Peek() >= 0)
            {
                secureString.AppendChar((char)reader.Read());
            }
            VerboseLog("[GetSecureStringFromParameterStore] Added to SecureString");

            return secureString;
        }

        /// <summary>
        /// GET file from AWS S3
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFileFromS3(string key)
        {
            VerboseLog("[GetFileFromS3] Getting file from S3 with Default Bucket");
            return await GetFileFromS3(key, S3BucketName);
        }

        public async Task<byte[]> GetFileFromS3(string key, string bucketName)
        {
            _logger.Debug("[AwsHelperV3] Inside GetFileFromS3.");
            var s3Client = GetS3Client();
            VerboseLog("[GetFileFromS3] Obtained S3 Client");

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            VerboseLog("[GetFileFromS3] Preparing to get object from S3");
            var response = await s3Client.GetObjectAsync(request);
            VerboseLog("[GetFileFromS3] Obtained object from S3");
            using Stream stream = response.ResponseStream;
            using MemoryStream inputStream = new MemoryStream();
            VerboseLog("[GetFileFromS3] Preparing to copy object to memory stream");
            await stream.CopyToAsync(inputStream);
            VerboseLog("[GetFileFromS3] Copied object to memory stream");
            return inputStream.ToArray();
        }

        /// <summary>
        /// GET secret from AWS Secrets Manager
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetSecretFromSecretsManager(string secretName)
        {
            _logger.Debug("GetSecretFromSecretsManager(" + secretName + ")");

            var secretsManagerClient = GetSecretsManagerClient();
            VerboseLog("[GetSecretFromSecretsManager] Get SSM Client");

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            try
            {
                _logger.Debug("Calling secrets manager client");
                VerboseLog("[GetSecretFromSecretsManager] Preparing to get Secret from SSM");
                var response = await secretsManagerClient.GetSecretValueAsync(request);
                VerboseLog("[GetSecretFromSecretsManager] Obtained Secret from SSM");

                if (response == null)
                {
                    _logger.Error("Unable to get secret from secrets manager");
                    return null;
                }

                _logger.Debug("Deserializing secert");
                _logger.Debug(response.SecretString);
                Dictionary<string, string> secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);
                VerboseLog("[GetSecretFromSecretsManager] Deserialized Secret");

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
