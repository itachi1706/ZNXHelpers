# SPC CS Helpers

SPCCSHelpers is a multi-targeted .NET helper library that centralises cross-cutting concerns used across SPC
applications. It packages reusable utilities for AWS integration, environment configuration, encoding, JWT handling,
secure random generation, and common exceptions so service code can stay focused on domain logic.

## Features

- `AwsHelperV3` (recommended) and `AwsHelperV2` wrap AWS SDK clients for Parameter Store, Secrets Manager, S3, and KMS.
  V3 adds richer logging, multiple credential sources (shared profiles, STS via EKS service accounts, basic
  credentials), and pre-signed URL generation.
- `EnvHelper` offers strongly-typed environment variable accessors with sensible defaults and helpers to detect the
  current ASP.NET Core environment.
- `JwtHelper` validates JSON Web Tokens and creates JSON Web Signatures (JWS) using certificates or arbitrary signing
  credentials.
- `Base64Helper` simplifies UTF-8 string encoding/decoding to Base64.
- `RngHelper` produces cryptographically secure random strings for secrets or temporary passwords.
- `HttpResponseException` carries HTTP status details for consistent API error responses.

## Getting Started

### Prerequisites

- .NET SDK 10.0 or later (library targets `net6.0`, `net7.0`, `net8.0`, and `net10.0`; the test project runs on
  `net8.0`).

### Install from NuGet

```bash
dotnet add package SPCCSHelpers --version 3.2.4
```

### Reference from Source

```bash
dotnet add <your-project>.csproj reference ../SPCCSHelpers/SPCCSHelpers.csproj
```

### Environment variables used by this library

| Variable                      | Used by                                                    |  Required   | Default                 | Purpose                                                                                       |
|-------------------------------|------------------------------------------------------------|:-----------:|-------------------------|-----------------------------------------------------------------------------------------------|
| `KMS_KEY_ID`                  | `AwsHelperV2`, `AwsHelperV3`                               |     No      | –                       | KMS key identifier used for decrypting secure Parameter Store values with encryption context. |
| `AWS_PROFILE_NAME`            | `AwsHelperV2`, `AwsHelperV3`                               |     No      | –                       | Uses the named shared credentials profile (typically local/dev usage).                        |
| `S3_BUCKET_NAME`              | `AwsHelperV2`, `AwsHelperV3`                               |     No      | –                       | Default S3 bucket for download/upload helpers and pre-signed URL generation.                  |
| `AWS_SECRET_NAME`             | `AwsHelperV3`                                              |     No      | –                       | Default Secrets Manager secret name used by `GetSecretFromSecretsManager()`.                  |
| `AWS_EKS_SA`                  | `AwsHelperV3`                                              |     No      | `false`                 | Enables STS/web-identity credential flow (EKS service account path).                          |
| `AWS_BASIC_AUTH`              | `AwsHelperV3`                                              |     No      | `false`                 | Force basic AWS credentials mode.                                                             |
| `AWS_ACCESS_KEY_ID`           | `AwsHelperV3`                                              | Conditional | –                       | Access key used when `AWS_BASIC_AUTH=true`.                                                   |
| `AWS_SECRET_ACCESS_KEY`       | `AwsHelperV3`                                              | Conditional | –                       | Secret key used when `AWS_BASIC_AUTH=true`.                                                   |
| `AWS_VERBOSE_DEBUG`           | `AwsHelperV3`                                              |     No      | `false`                 | Enables verbose AWS helper debug logging.                                                     |
| `AWS_PRINT_STACK_TRACE`       | `AwsHelperV3`                                              |     No      | `false`                 | Includes stack traces in AWS helper error logs.                                               |
| `AWS_REQUEST_ID_DEBUG`        | `AwsHelperV3`                                              |     No      | `false`                 | Logs AWS request IDs from response metadata.                                                  |
| `AWS_RESPONSE_METADATA_DEBUG` | `AwsHelperV3`                                              |     No      | `false`                 | Logs full AWS response metadata dictionaries for troubleshooting.                             |
| `AWS_CUSTOM_METRICS`          | `AwsHelperV3`, `MetricQueue`, `CloudwatchMetricsPublisher` |     No      | `false`                 | Enables CloudWatch custom metrics queueing and publishing support.                            |
| `METRICS_NAMESPACE`           | `AwsHelperV3`                                              |     No      | `App/SPCCSGateway`      | Default CloudWatch namespace when none is supplied at call time.                              |
| `METRICS_VERBOSE_LOGGING`     | `CloudwatchMetricsPublisher`                               |     No      | `false`                 | Enables verbose logs in the background metrics publisher loop.                                |
| `METRICS_ALWAYS_INSTANCE_ID`  | `CloudwatchMetricsPublisher`                               |     No      | `true`                  | Auto-adds `UniqueIdentifier` dimension if missing from queued metrics.                        |
| `APP_ID`                      | `AwsHelperV3`                                              |     No      | `HOSTNAME` variable     | Preferred unique instance identifier used for metric dimensions.                              |
| `HOSTNAME`                    | `AwsHelperV3`                                              |     No      | `MACHINE_NAME` variable | Fallback unique instance identifier when `APP_ID` is not set.                                 |
| `ASPNETCORE_ENVIRONMENT`      | `EnvHelper`, `NdiHelper`                                   |     No      | `Development`           | Controls environment detection helpers and NDI HTTPS validation behavior.                     |
| `API_GATEWAY_KEY`             | `NdiHelper`                                                |     No      | empty string            | Adds `x-api-key` header for NDI/API-gateway requests when provided.                           |

Notes:

- `Required` is `Conditional` when the variable is only required by a specific mode/flag.
- If both `APP_ID` and `HOSTNAME` are empty, metric instance identity falls back to the machine name.

## Usage

### AWS helper (v3)

```csharp
using SPCCSHelpers;

var aws = new AwsHelperV3();

var parameterValue = await aws.GetStringFromParameterStore("/my-service/config");
var secretValues = await aws.GetSecretFromSecretsManager(); // returns Dictionary<string, string>
var fileBytes = await aws.GetFileFromS3("exports/report.csv");
var uploadSucceeded = await aws.PutFileToS3(Encoding.UTF8.GetBytes("content"), "exports/new-report.csv", "text/csv");
var downloadUrl = aws.GeneratePreSignedS3UrlDownload("exports/report.csv", expiryMin: 30);
```

`AwsHelperV3` automatically picks credential sources in the following order:

1. Shared credentials profile (when `AWS_PROFILE_NAME` is supplied).
2. STS Assume Role With Web Identity when running in EKS with `AWS_EKS_SA=true`.
3. Basic access key authentication when `AWS_BASIC_AUTH=true`.
4. Default environment/instance profile credentials.

All AWS operations default to the `ap-southeast-1` region and fall back to the shared environment bucket/secret names
when provided.

For more information on the environment variables, please
see [Environment variables used by this library](#environment-variables-used-by-this-library) above.

`AwsHelperV2` remains available for consumers that prefer the earlier, profile-only client creation behaviour.

### Environment helper

```csharp
using SPCCSHelpers;

var isDevelopment = EnvHelper.IsDevelopmentEnvironment();
var redisPort = EnvHelper.GetInt("REDIS_PORT", 6379);
var allowedOrigins = EnvHelper.GetStringList("ALLOWED_ORIGINS") ?? new List<string>();
```

### JWT helper

```csharp
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SPCCSHelpers;

var helper = new JwtHelper();
var validationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    ValidateIssuer = false,
    ValidateAudience = false,
};

if (helper.ValidateJwt(token, validationParameters, out var claims, out var securityToken))
{
    // token is valid, inspect claims
}

var jws = helper.CreateJws(
    issuer: "example",
    audience: "clients",
    claims: new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") },
    notBefore: DateTime.UtcNow,
    expires: DateTime.UtcNow.AddMinutes(15),
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        SecurityAlgorithms.HmacSha256)
);
```

### Base64 and RNG helpers

```csharp
using SPCCSHelpers;

var base64 = new Base64Helper();
var encoded = base64.EncodeString("hello world");
var decoded = base64.DecodeString(encoded);

var rng = new RngHelper();
var password = rng.GeneratePassword(32);
```

### HttpResponseException

```csharp
using Microsoft.AspNetCore.Http;
using SPCCSHelpers.Exceptions;

throw new HttpResponseException(StatusCodes.Status403Forbidden, "Missing permission");
```

### Cloudwatch Custom Metrics Helper

Initialize in Program.cs of your application the following. Note the order must follow these.

```csharp
builder.Services.AddSingleton<AwsHelperV3>(); // required
builder.Services.AddSingleton<MetricQueue>(); // For common queue
builder.Services.AddHostedService<CloudwatchMetricsPublisher>();
```

After that, in any of your classes requiring metrics injection, you can simply do the following:

```csharp
public class MyClass
{
    private readonly MetricQueue _metricQueue;

    // MetricQueue is injected via DI and can be used to enqueue custom metrics
    public MyClass(MetricQueue metricQueue)
    {
        _metricQueue = metricQueue;
    }

    public void MyMethod()
    {
        // Enqueue a custom metric with dimension
        _metricQueue.EnqueueMetric("MyCustomMetric", 1, new Dictionary<string, string>
        {
            { "Environment", "Production" },
            { "Service", "MyService" }
        });
        
        // Enqueue a custom metric without dimension
        _metricQueue.EnqueueMetric("MySimpleMetric", 1);
    }
}
```

## Development

```bash
# Restore dependencies
dotnet restore SPCCSHelpers.sln

# Build the library
dotnet build SPCCSHelpers.sln

# Run the test suite (xUnit + Moq + coverlet)
dotnet test SPCCSHelpers.sln

# Optional: collect coverage
dotnet test SPCCSHelpers.sln /p:CollectCoverage=true
```

Use `dotnet pack SPCCSHelpers/SPCCSHelpers.csproj -c Release` to produce a NuGet package when you are ready to publish.

## Additional Resources

- `CHANGELOG.md` tracks notable changes across releases.
- `SPCCSHelpers.Tests` includes samples that demonstrate how each helper is exercised.
