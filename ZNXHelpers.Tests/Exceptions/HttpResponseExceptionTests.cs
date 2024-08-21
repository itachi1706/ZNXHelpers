using ZNXHelpers.Exceptions;

namespace ZNXHelpers.Tests.Exceptions;

public class HttpResponseExceptionTests
{
    [Fact]
    public void TestHttpResponseExceptionWithStatusCodeOnly()
    {
        // Arrange
        var statusCode = 404;

        // Act
        var exception = new HttpResponseException(statusCode);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Null(exception.CustomMessage);
        Assert.Equal($"HTTP Response Status Code {statusCode}.", exception.Message);
    }

    [Fact]
    public void TestHttpResponseExceptionWithStatusCodeAndCustomMessage()
    {
        // Arrange
        var statusCode = 500;
        var customMessage = "Internal Server Error";

        // Act
        var exception = new HttpResponseException(statusCode, customMessage);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Equal(customMessage, exception.CustomMessage);
        Assert.Equal($"{customMessage} : HTTP Response Status Code {statusCode}.", exception.Message);
    }
}