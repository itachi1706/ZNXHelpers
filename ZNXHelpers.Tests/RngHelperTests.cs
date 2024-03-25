namespace ZNXHelpers.Tests;

public class RngHelperTests
{
    private readonly RngHelper _rngHelper;

    public RngHelperTests()
    {
        _rngHelper = new RngHelper();
    }

    [Fact]
    public void TestGeneratePassword()
    {
        // Arrange
        var size = 16;

        // Act
        var result = _rngHelper.GeneratePassword(size);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(size+8, result.Length);
    }
}