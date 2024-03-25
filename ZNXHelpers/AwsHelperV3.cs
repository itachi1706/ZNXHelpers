using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.SecurityToken;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Newtonsoft.Json;
using Serilog;
using System.Security;
using System.Text;
using ResourceNotFoundException = Amazon.SimpleSystemsManagement.Model.ResourceNotFoundException;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace ZNXHelpers
{
    public class AwsHelperV3
    {
        private readonly string? _kmsKeyId = EnvHelper.GetString("KMS_KEY_ID");
        private readonly string? _profileName = EnvHelper.GetString("AWS_PROFILE_NAME");
        private readonly string? _s3BucketName = EnvHelper.GetString("S3_BUCKET_NAME");
        private readonly string? _secretName = EnvHelper.GetString("AWS_SECRET_NAME");
        private readonly bool _isAwsEksSa = EnvHelper.GetBool("AWS_EKS_SA", false);
        private readonly bool _isAwsBasicAuth = EnvHelper.GetBool("AWS_BASIC_AUTH", false);
        private readonly bool _verboseLogEnabled = EnvHelper.GetBool("AWS_VERBOSE_DEBUG", false);
        private readonly bool _awsPrintStackTrace = EnvHelper.GetBool("AWS_PRINT_STACK_TRACE", false);
        private readonly bool _awsRequestIdLoggingEnabled = EnvHelper.GetBool("AWS_REQUEST_ID_DEBUG", false);
        private readonly bool _awsMetadataLoggingEnabled = EnvHelper.GetBool("AWS_RESPONSE_METADATA_DEBUG", false);
        private readonly ILogger _logger;

        /** TESTS ONLY **/
        private readonly AmazonS3Client? _testS3Client;
        private readonly AmazonKeyManagementServiceClient? _testKmsClient;
        private readonly AmazonSecretsManagerClient? _testSecretsManagerClient;
        private readonly AmazonSimpleSystemsManagementClient? _testSimpleSystemsManagementClient;
        private readonly bool _testMode;
        public AwsHelperV3(AmazonS3Client s3Client, AmazonKeyManagementServiceClient kmsClient, AmazonSecretsManagerClient smClient, AmazonSimpleSystemsManagementClient ssmClient)
        {
            _logger = Log.ForContext<AwsHelperV3>();
            _testS3Client = s3Client;
            _testKmsClient = kmsClient;
            _testSecretsManagerClient = smClient;
            _testSimpleSystemsManagementClient = ssmClient;
            _testMode = true;
        }

        public AwsHelperV3()
        {
            _logger = Log.ForContext<AwsHelperV3>();
        }

        #region Utils
        private void VerboseLog(string? log)
        {
            if (log == null) return; // NO-OP
            if (_verboseLogEnabled)
            {
                _logger.Debug("{Log}", log);
            }
        }
        
        private void LogMetadata(ResponseMetadata metadata, String tag) 
        {
            if (_awsRequestIdLoggingEnabled)
            {
                _logger.Information("[{Tag}] Request ID: {RequestId}", tag, metadata.RequestId);
            }
            if (_awsMetadataLoggingEnabled)
            {
                _logger.Information("[{Tag}] Metadata: {@Metadata}", tag, metadata.Metadata);
            }
        }
        #endregion

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

        private async Task<AWSCredentials?> GetAwsCredentialsSts()
        {
            var credentialsDebug = AssumeRoleWithWebIdentityCredentials.FromEnvironmentVariables();
            VerboseLog("[GetAwsCredentialsSts] Getting Creds Async WebIdentity");
            var debugCreds = credentialsDebug.GetCredentials();
            var ak = debugCreds.AccessKey ?? "-";
            var sk = debugCreds.SecretKey ?? "-";
            var tk = debugCreds.Token ?? "-";
            VerboseLog($"[GetAwsCredentialsSts] Creds Async gotten WebIdentity. {ak}, {sk}, {tk}");

            IAmazonSecurityTokenService stsClient = new AmazonSecurityTokenServiceClient(Amazon.RegionEndpoint.APSoutheast1);
            AWSCredentials stsUser;
            using (var client = stsClient)
            {
                VerboseLog("[GetAwsCredentialsSts] Getting STS Session Token");
                VerboseLog("[GetAwsCredentialsSts] Getting STS Session Token");
                var token = await client.GetSessionTokenAsync();
                VerboseLog("[GetAwsCredentialsSts] Obtained STS Session Token");
                stsUser = token.Credentials;
            }
            VerboseLog("[GetAwsCredentialsSts] Returning STS User");
            return stsUser;
        }

        private static AWSCredentials GetBasicAwsCredentials()
        {
            var awsAk = EnvHelper.GetString("AWS_ACCESS_KEY_ID");
            var awsSk = EnvHelper.GetString("AWS_SECRET_ACCESS_KEY");
            if (string.IsNullOrEmpty(awsAk) || string.IsNullOrEmpty(awsSk))
            {
                throw new AmazonServiceException("Failed to get basic AWS Credentials");
            }

            return new BasicAWSCredentials(awsAk, awsSk);
        }

        private AWSCredentials? GetProdCredentials()
        {
            if (_isAwsEksSa)
            {
                VerboseLog("[GetProdCredentials] Getting STS credentials");
                var user = GetAwsCredentialsSts();
                user.Wait();

                VerboseLog("[GetProdCredentials] Returning STS prod client");
                return user.Result;
            }
            else if (_isAwsBasicAuth)
            {
                VerboseLog("[GetProdCredentials] Getting Basic Auth credentials");
                return GetBasicAwsCredentials();
            }

            return null;
        }
        #endregion

        #region AWS Clients
        /********** CLIENTS **********/
        private AmazonKeyManagementServiceClient GetKmsClient()
        {
            if (_testMode) return _testKmsClient!;
            return _profileName == null ?
                GetKmsClientProd() :
                new AmazonKeyManagementServiceClient(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonKeyManagementServiceClient GetKmsClientProd()
        {
            var credentials = GetProdCredentials();
            if (credentials != null)
            {
                VerboseLog("[GetKmsClientProd] Returning client with credentials");
                return new AmazonKeyManagementServiceClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetKmsClientProd] Returning normal prod client");
            return new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonS3Client GetS3Client()
        {
            if (_testMode) return _testS3Client!;
            return _profileName == null ?
                GetS3ClientProd() :
                new AmazonS3Client(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonS3Client GetS3ClientProd()
        {
            var credentials = GetProdCredentials();
            if (credentials != null)
            {
                VerboseLog("[GetS3ClientProd] Returning client with credentials");
                return new AmazonS3Client(credentials, Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetS3ClientProd] Returning normal prod client");
            return new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSecretsManagerClient GetSecretsManagerClient()
        {
            if (_testMode) return _testSecretsManagerClient!;
            return _profileName == null ?
                GetSecretsManagerClientProd() :
                new AmazonSecretsManagerClient(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSecretsManagerClient GetSecretsManagerClientProd()
        {
            var creds = GetProdCredentials();
            if (creds != null)
            {
                VerboseLog("[GetSecretsManagerClientProd] Returning client with credentials");
                return new AmazonSecretsManagerClient(creds, Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetSecretsManagerClientProd] Returning normal prod client");
            return new AmazonSecretsManagerClient(Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSimpleSystemsManagementClient GetSimpleSystemsManagementClient()
        {
            if (_testMode) return _testSimpleSystemsManagementClient!;
            return _profileName == null ?
                GetSimpleSystemsManagementClientProd() :
                new AmazonSimpleSystemsManagementClient(GetAwsCredentials(_profileName), Amazon.RegionEndpoint.APSoutheast1);
        }

        private AmazonSimpleSystemsManagementClient GetSimpleSystemsManagementClientProd()
        {
            var credentials = GetProdCredentials();
            if (credentials != null)
            {
                VerboseLog("[GetSimpleSystemsManagementClientProd] Returning client with credentials");
                return new AmazonSimpleSystemsManagementClient(credentials, Amazon.RegionEndpoint.APSoutheast1);
            }

            VerboseLog("[GetSimpleSystemsManagementClientProd] Returning normal prod client");
            return new AmazonSimpleSystemsManagementClient(Amazon.RegionEndpoint.APSoutheast1);
        }
        #endregion

        #region AWS Methods
        #region AWS Parameter Store
        /// <summary>
        /// GET string from AWS Parameter Store
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public async Task<string?> GetStringFromParameterStore(string parameterName)
        {
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName
            };

            VerboseLog("[GetStringFromParameterStore] Getting String from Parameter Store");
            var response = await ssmClient.GetParameterAsync(request);
            LogMetadata(response.ResponseMetadata, "GetStringFromParameterStore");
            VerboseLog("[GetStringFromParameterStore] Obtained String from Parameter Store");

            if (response.Parameter == null)
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
        public async Task<string?> GetStringFromParameterStoreSecureString(string parameterName, bool withDecryption)
        {
            _logger.Debug("GetStringFromParameterStoreSecureString({ParameterName}, {Decryption})", parameterName, withDecryption);
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = withDecryption
            };

            VerboseLog("[GetStringFromParameterStoreSecureString] Getting Secure String from Parameter Store");
            var response = await ssmClient.GetParameterAsync(request);
            LogMetadata(response.ResponseMetadata, "GetStringFromParameterStoreSecureString");
            VerboseLog("[GetStringFromParameterStoreSecureString] Obtained Secure String from Parameter Store");

            if (response.Parameter == null)
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

            using var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));
            VerboseLog("[GetStringFromParameterStoreSecureString] Generated SecureString Stream");

            var decryptRequest = new DecryptRequest
            {
                KeyId = _kmsKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    { "PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            VerboseLog("[GetStringFromParameterStoreSecureString] Getting KMS Client");
            var kmsClient = GetKmsClient();

            VerboseLog("[GetStringFromParameterStoreSecureString] Prepare to decrypt with KMS Client");
            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);
            LogMetadata(decryptResponse.ResponseMetadata, "GetStringFromParameterStoreSecureString");
            VerboseLog("[GetStringFromParameterStoreSecureString] Decrypted String with KMS Client");

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
            using var ssmClient = GetSimpleSystemsManagementClient();

            var request = new GetParameterRequest
            {
                Name = parameterName,
                WithDecryption = false
            };

            VerboseLog("[GetSecureStringFromParameterStore] Getting Secure String from Parameter Store");
            var response = await ssmClient.GetParameterAsync(request);
            LogMetadata(response.ResponseMetadata, "GetSecureStringFromParameterStore");
            VerboseLog("[GetSecureStringFromParameterStore] Obtained Secure String from Parameter Store");

            if (response.Parameter == null)
            {
                _logger.Error("Unable to get secure string from parameter store");
                return null;
            }

            var encryptedValue = response.Parameter.Value;

            using var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedValue));
            VerboseLog("[GetSecureStringFromParameterStore] Generated SecureString Stream");

            var decryptRequest = new DecryptRequest
            {
                KeyId = _kmsKeyId,
                CiphertextBlob = memoryStream,
                EncryptionContext = new Dictionary<string, string>  // For parameter store secure string, add context to decrypt successfully
                {
                    {"PARAMETER_ARN", response.Parameter.ARN }
                }
            };

            VerboseLog("[GetSecureStringFromParameterStore] Getting KMS Client");
            var kmsClient = GetKmsClient();

            VerboseLog("[GetSecureStringFromParameterStore] Preparing to decrypt string with KMS Client");
            var decryptResponse = await kmsClient.DecryptAsync(decryptRequest);
            LogMetadata(decryptResponse.ResponseMetadata, "GetSecureStringFromParameterStore");
            VerboseLog("[GetSecureStringFromParameterStore] Decrypted String with KMS Client");

            var secureString = new SecureString();

            using var reader = new StreamReader(decryptResponse.Plaintext);
            VerboseLog("[GetSecureStringFromParameterStore] Converted decrypted string to a stream for insertion to SecureString");

            while (reader.Peek() >= 0)
            {
                secureString.AppendChar((char)reader.Read());
            }
            VerboseLog("[GetSecureStringFromParameterStore] Added to SecureString");

            return secureString;
        }
        #endregion

        #region AWS S3
        /// <summary>
        /// GET file from AWS S3
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFileFromS3(string key)
        {
            VerboseLog("[GetFileFromS3] Getting file from S3 with Default Bucket");
            return await GetFileFromS3(key, _s3BucketName);
        }

        public async Task<byte[]> GetFileFromS3(string key, string? bucketName)
        {
            _logger.Debug("[AwsHelperV3] Inside GetFileFromS3");
            var s3Client = GetS3Client();
            VerboseLog("[GetFileFromS3] Obtained S3 Client");

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            VerboseLog("[GetFileFromS3] Preparing to get object from S3");
            var response = await s3Client.GetObjectAsync(request);
            LogMetadata(response.ResponseMetadata, "GetFileFromS3");
            VerboseLog("[GetFileFromS3] Obtained object from S3");
            await using var stream = response.ResponseStream;
            using var inputStream = new MemoryStream();
            VerboseLog("[GetFileFromS3] Preparing to copy object to memory stream");
            await stream.CopyToAsync(inputStream);
            VerboseLog("[GetFileFromS3] Copied object to memory stream");
            return inputStream.ToArray();
        }
        
        public async Task<bool> PutFileToS3(byte[] file, string filePath)
        {
            VerboseLog("[PutFileToS3] Putting file into S3 with Default Bucket with default content (text/plain)");
            return await PutFileToS3(file, filePath, "text/plain");
        }

        public async Task<bool> PutFileToS3(byte[] file, string filePath, string contentType)
        {
            VerboseLog("[PutFileToS3] Putting file into S3 with Default Bucket");
            return await PutFileToS3(file, filePath, contentType, _s3BucketName);
        }

        public async Task<bool> PutFileToS3(byte[] file, string filePath, string contentType, string? bucketName)
        {
            VerboseLog("[PutFileToS3] Start");
            var s3Client = GetS3Client();
            VerboseLog("[PutFileToS3] Obtained S3 Client");

            using var stream = new MemoryStream(file);
            var request = new PutObjectRequest
            {
                InputStream = stream,
                BucketName = bucketName,
                Key = filePath,
                ContentType = contentType
            };

            VerboseLog("[PutFileToS3] Uploading to S3 bucket...");
            try {
                var response = await s3Client.PutObjectAsync(request);
                LogMetadata(response.ResponseMetadata, "PutFileToS3");
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    VerboseLog("[PutFileToS3] Successfully uploaded to S3 bucket");
                    return true;
                }

                VerboseLog("[PutFileToS3] Failed to upload to S3 bucket");
                return false;
            } catch (AmazonS3Exception ex) {
                VerboseLog("[PutFileToS3] Failed to upload to S3 bucket. Exception: " + ex.Message);
                VerboseLog("[PutFileToS3] Request ID: " + ex.RequestId);
                if (_awsPrintStackTrace) {
                    VerboseLog(ex.StackTrace);
                }
                return false;
            }
        }

        public string? GeneratePreSignedS3UrlDownload(string filePath, long expiryMin)
	    {
            VerboseLog("[PutFileToS3] Getting Pre Signed URL with Default Bucket");
            return GeneratePreSignedS3UrlDownload(filePath, expiryMin, _s3BucketName);
        }

        public string? GeneratePreSignedS3UrlDownload(string filePath, long expiryMin, string? bucketName)
        {
            VerboseLog("[GeneratePreSignedS3URLDownload] Start");
            // Make sure must be less than 7 days (Ref: https://docs.aws.amazon.com/AmazonS3/latest/userguide/ShareObjectPreSignedURL.html)
            // 7 days = 10080 minutes
            if (expiryMin > 10080)
            {
                throw new AmazonS3Exception("Expiry cannot be greater than 7 days from time of creation");
            }

            var s3Client = GetS3Client();
            VerboseLog("[GeneratePreSignedS3URLDownload] Obtained S3 Client");

            var req = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = filePath,
                Expires = DateTime.UtcNow.AddMinutes(expiryMin)
            };

            try
			{
                VerboseLog("[GeneratePreSignedS3URLDownload] Retrieving Pre-Signed URL");
                var url = s3Client.GetPreSignedURL(req);
                return url;
            } catch (AmazonS3Exception e)
			{
                VerboseLog($"[GeneratePreSignedS3URLDownload] Error encountered on server generating pre-signed URL. Message: '{e.Message}' when writing an object");
                VerboseLog("[GeneratePreSignedS3URLDownload] Request ID: " + e.RequestId);
			} catch (Exception e)
			{
                VerboseLog($"[GeneratePreSignedS3URLDownload] Unknown Exception encountered generating pre-signed URL. Message: '{e.Message}' when writing an object");
            }
            
            return null;
		}
        #endregion

        #region AWS Secrets Manager
        
        /// <summary>
        /// Get secrets from AWS secrets manager using default secret name
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, string>?> GetSecretFromSecretsManager()
        {
            VerboseLog($"[GetSecretFromSecretsManager] Getting secrets from AWS secrets manager using default secret name {_secretName}.");
            return await GetSecretFromSecretsManager(_secretName);
        }

        /// <summary>
        /// Get secrets from AWS secrets manager
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>?> GetSecretFromSecretsManager(string? secretName)
        {
            _logger.Debug("GetSecretFromSecretsManager({SecretName})", secretName);

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
                LogMetadata(response.ResponseMetadata, "GetSecretFromSecretsManager");
                VerboseLog("[GetSecretFromSecretsManager] Obtained Secret from SSM");

                _logger.Debug("Deserializing secert");
                _logger.Debug("{SS}", response.SecretString);
                var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.SecretString);
                VerboseLog("[GetSecretFromSecretsManager] Deserialized Secret");

                _logger.Debug("Returning secrets");
                return secrets;
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to get secret from secrets manager exception");
                return null;
            }
            catch (ResourceNotFoundException ex2)
            {
                _logger.Error(ex2, "Unable to get secret from secrets manager");
                return null;   
            }
        }
        #endregion
        #endregion
    }
}
