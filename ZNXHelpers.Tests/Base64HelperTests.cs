namespace ZNXHelpers.Tests;

public class Base64HelperTests
{
    private readonly Base64Helper _base64Helper;

    public Base64HelperTests()
    {
        _base64Helper = new Base64Helper();
    }

    [Fact]
    public void TestEncodeString()
    {
        // Arrange
        var input = "test string";
        var expected = "dGVzdCBzdHJpbmc="; // This is "test string" encoded in base64

        // Act
        var result = _base64Helper.EncodeString(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TestDecodeString()
    {
        // Arrange
        var input = "dGVzdCBzdHJpbmc="; // This is "test string" encoded in base64
        var expected = "test string";

        // Act
        var result = _base64Helper.DecodeString(input);

        // Assert
        Assert.Equal(expected, result);
    }
}