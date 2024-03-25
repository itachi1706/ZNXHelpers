namespace ZNXHelpers.Tests;

public class EnvHelperTests
{
    [Fact]
    public void TestIsDevelopmentEnvironment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        // Act
        var result = EnvHelper.IsDevelopmentEnvironment();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TestIsProductionEnvironment()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

        // Act
        var result = EnvHelper.IsProductionEnvironment();

        // Assert
        Assert.True(result);
    }

    // Add more tests for other methods in the EnvHelper class
}