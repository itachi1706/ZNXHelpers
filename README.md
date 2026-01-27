
# ZNXHelpers

ZNXHelpers is a multi-targeted .NET helper library that centralises cross-cutting concerns used across ZNX applications. It packages reusable utilities for AWS integration, environment configuration, encoding, JWT handling, secure random generation, and common exceptions so service code can stay focused on domain logic.

## Features
- `AwsHelperV3` (recommended) and `AwsHelperV2` wrap AWS SDK clients for Parameter Store, Secrets Manager, S3, and KMS. V3 adds richer logging, multiple credential sources (shared profiles, STS via EKS service accounts, basic credentials), and pre-signed URL generation.
- `EnvHelper` offers strongly-typed environment variable accessors with sensible defaults and helpers to detect the current ASP.NET Core environment.
- `JwtHelper` validates JSON Web Tokens and creates JSON Web Signatures (JWS) using certificates or arbitrary signing credentials.
- `Base64Helper` simplifies UTF-8 string encoding/decoding to Base64.
- `RngHelper` produces cryptographically secure random strings for secrets or temporary passwords.
- `HttpResponseException` carries HTTP status details for consistent API error responses.

## Getting Started

### Prerequisites
- .NET SDK 10.0 or later (library targets `net6.0`, `net7.0`, `net8.0`, and `net10.0`; the test project runs on `net8.0`).

### Install from NuGet

```bash
dotnet add package ZNXHelpers --version 3.0.2
```

### Reference from Source

```bash
dotnet add <your-project>.csproj reference ../ZNXHelpers/ZNXHelpers.csproj
```

## Usage

### AWS helper (v3)

```csharp
using ZNXHelpers;

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

All AWS operations default to the `ap-southeast-1` region and fall back to the shared environment bucket/secret names when provided.

#### AWS-related environment variables

| Variable | Required | Default | Purpose |
| --- | :---: | --- | --- |
| `KMS_KEY_ID` | No | – | Adds encryption context for secure Parameter Store values. |
| `AWS_PROFILE_NAME` | No | – | Uses the named profile from local credentials for development. |
| `S3_BUCKET_NAME` | No | – | Default bucket for S3 downloads/uploads. |
| `AWS_SECRET_NAME` | No | – | Default Secrets Manager secret to retrieve. |
| `AWS_EKS_SA` | No | `false` | Enables STS AssumeRoleWithWebIdentity flow for EKS service accounts. |
| `AWS_BASIC_AUTH` | No | `false` | Forces usage of basic credentials (`AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY`). |
| `AWS_ACCESS_KEY_ID` | Conditional | – | Access key used when `AWS_BASIC_AUTH=true`. |
| `AWS_SECRET_ACCESS_KEY` | Conditional | – | Secret key used when `AWS_BASIC_AUTH=true`. |
| `AWS_VERBOSE_DEBUG` | No | `false` | Emits verbose Serilog debug entries. |
| `AWS_PRINT_STACK_TRACE` | No | `false` | Logs AWS exception stack traces when failures occur. |
| `AWS_REQUEST_ID_DEBUG` | No | `false` | Logs AWS response request identifiers. |
| `AWS_RESPONSE_METADATA_DEBUG` | No | `false` | Logs additional AWS response metadata for troubleshooting. |

`AwsHelperV2` remains available for consumers that prefer the earlier, profile-only client creation behaviour.

### Environment helper

```csharp
using ZNXHelpers;

var isDevelopment = EnvHelper.IsDevelopmentEnvironment();
var redisPort = EnvHelper.GetInt("REDIS_PORT", 6379);
var allowedOrigins = EnvHelper.GetStringList("ALLOWED_ORIGINS") ?? new List<string>();
```

### JWT helper

```csharp
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using ZNXHelpers;

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
using ZNXHelpers;

var base64 = new Base64Helper();
var encoded = base64.EncodeString("hello world");
var decoded = base64.DecodeString(encoded);

var rng = new RngHelper();
var password = rng.GeneratePassword(32);
```

### HttpResponseException

```csharp
using Microsoft.AspNetCore.Http;
using ZNXHelpers.Exceptions;

throw new HttpResponseException(StatusCodes.Status403Forbidden, "Missing permission");
```

## Development

```bash
# Restore dependencies
dotnet restore ZNXHelpers.sln

# Build the library
dotnet build ZNXHelpers.sln

# Run the test suite (xUnit + Moq + coverlet)
dotnet test ZNXHelpers.sln

# Optional: collect coverage
dotnet test ZNXHelpers.sln /p:CollectCoverage=true
```

Use `dotnet pack ZNXHelpers/ZNXHelpers.csproj -c Release` to produce a NuGet package when you are ready to publish.

## Additional Resources
- `CHANGELOG.md` tracks notable changes across releases.
- `qodana.yaml` holds static analysis rules for JetBrains Qodana.
- `ZNXHelpers.Tests` includes samples that demonstrate how each helper is exercised.
