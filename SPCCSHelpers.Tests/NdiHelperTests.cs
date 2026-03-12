using Serilog;

namespace SPCCSHelpers.Tests;

public class NdiHelperTests
{
    private readonly NdiHelper _ndiHelper;

    public NdiHelperTests()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .CreateLogger();
        
        _ndiHelper = new NdiHelper();
    }

    [Fact]
    public async Task CallNdiEndpoint_InvalidUrl_ReturnsNull()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://invalid-domain-that-does-not-exist-12345.com";
        var endpointPath = "/test/endpoint";
        var accessToken = "token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, true);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task CallNdiEndpoint_PostMethod_WithHttpBinEcho_ReturnsResponse()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/post";
        var accessToken = "test-token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, true, true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("httpbin.org", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_GetMethod_WithHttpBinGet_ReturnsResponse()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/get";
        var accessToken = "test-token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("httpbin.org", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_LambdaHeaderModeTrue_IncludesAuthInFormData()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/post";
        var accessToken = "lambda-auth-token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, true, true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("auth", result);
        Assert.Contains("lambda-auth-token", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_LambdaHeaderModeFalse_IncludesAuthorizationHeader()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/get";
        var accessToken = "bearer-auth-token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Bearer bearer-auth-token", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_WithApiKey_IncludesApiKeyInHeaders()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "test-api-key-123");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/get";
        var accessToken = "token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("X-Api-Key", result);
        Assert.Contains("test-api-key-123", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_WithEmptyApiKey_DoesNotIncludeApiKeyHeader()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/get";
        var accessToken = "token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
        Assert.DoesNotContain("X-Api-Key", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_EmptyAccessToken_WithLambdaMode_SendsEmptyAuthData()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/post";
        var accessToken = "";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, true, true);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("\"auth\": \"\"", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_EmptyAccessToken_WithBearerMode_SendsEmptyBearer()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/get";
        var accessToken = "";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Bearer", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_PostMethod_DefaultParameter_UsesPostRequest()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/post";
        var accessToken = "token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("httpbin.org", result);
    }

    [Fact]
    public async Task CallNdiEndpoint_NotFoundStatusCode_ReturnsResponseContent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/status/404";
        var accessToken = "token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CallNdiEndpoint_UnauthorizedStatusCode_ReturnsResponseContent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/status/401";
        var accessToken = "invalid-token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CallNdiEndpoint_SuccessStatusCode_ReturnsResponseContent()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        Environment.SetEnvironmentVariable("API_GATEWAY_KEY", "");
        
        var baseUrl = "https://httpbin.org";
        var endpointPath = "/status/200";
        var accessToken = "token";

        // Act
        var result = await _ndiHelper.CallNdiEndpoint(baseUrl, endpointPath, accessToken, false, false);

        // Assert
        Assert.NotNull(result);
    }
}

