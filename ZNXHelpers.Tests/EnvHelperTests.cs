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

    [Fact]
    public void TestStringEnv()
    {
        Environment.SetEnvironmentVariable("TEST1", "haha");
        
        var resultValid = EnvHelper.GetString("TEST1");
        var resultInvalid = EnvHelper.GetString("TEST2");
        var resultDefault = EnvHelper.GetString("TEST3", "yes");
        
        Assert.Equal("haha", resultValid);
        Assert.Null(resultInvalid);
        Assert.Equal("yes", resultDefault);
    }
    
    [Fact]
    public void TestIntEnv()
    {
        Environment.SetEnvironmentVariable("TEST1", "1");
        
        var resultValid = EnvHelper.GetInt("TEST1");
        var resultDefault = EnvHelper.GetInt("TEST3", 3);
        
        Assert.Equal(1, resultValid);
        Assert.Throws<ArgumentNullException>(() => EnvHelper.GetInt("TEST2"));
        Assert.Equal(3, resultDefault);
    }

    [Fact]
    public void TestLongEnv()
    {
        Environment.SetEnvironmentVariable("TEST1", "1");
        
        var resultValid = EnvHelper.GetLong("TEST1");
        var resultDefault = EnvHelper.GetLong("TEST3", 3);
        
        Assert.Equal(1, resultValid);
        Assert.Throws<ArgumentNullException>(() => EnvHelper.GetLong("TEST2"));
        Assert.Equal(3, resultDefault);
    }
    
    [Fact]
    public void TestDoubleEnv()
    {
        Environment.SetEnvironmentVariable("TEST1", "1.3");
        
        var resultValid = EnvHelper.GetDouble("TEST1");
        var resultDefault = EnvHelper.GetDouble("TEST3", 3.2);
        
        Assert.Equal(1.3, resultValid);
        Assert.Throws<ArgumentNullException>(() => EnvHelper.GetDouble("TEST2"));
        Assert.Equal(3.2, resultDefault);
    }
    
    [Fact]
    public void TestBoolEnv()
    {
        Environment.SetEnvironmentVariable("TEST1", "true");
        
        var resultValid = EnvHelper.GetBool("TEST1");
        var resultDefault = EnvHelper.GetBool("TEST3", false);
        
        Assert.True(resultValid);
        Assert.Throws<ArgumentNullException>(() => EnvHelper.GetBool("TEST2"));
        Assert.False(resultDefault);
    }
    
    [Fact]
    public void TestStringArr()
    {
        Environment.SetEnvironmentVariable("TEST1", "1,2,3");
        Environment.SetEnvironmentVariable("TEST5", "1-2-3");
        
        var resultValid = EnvHelper.GetStringArr("TEST1");
        var resultNull = EnvHelper.GetStringArr("TEST2");
        var resultSingle = EnvHelper.GetStringArr("TEST5");
        var resultDefault = EnvHelper.GetStringArr("TEST5", '-');
        
        Assert.Equivalent(new[] {"1", "2", "3"}, resultValid);
        Assert.Equivalent(new[] {"1", "2", "3"}, resultDefault);
        Assert.Null(resultNull);
        Assert.Equivalent(new[] {"1-2-3"}, resultSingle);
    }
    
    [Fact]
    public void TestStringList()
    {
        Environment.SetEnvironmentVariable("TEST1", "1,2,3");
        Environment.SetEnvironmentVariable("TEST5", "1-2-3");
        
        var resultValid = EnvHelper.GetStringList("TEST1");
        var resultNull = EnvHelper.GetStringList("TEST2");
        var resultSingle = EnvHelper.GetStringList("TEST5");
        var resultDefault = EnvHelper.GetStringList("TEST5", '-');
        
        Assert.Equivalent(new List<string> {"1", "2", "3"}, resultValid);
        Assert.Equivalent(new List<string> {"1", "2", "3"}, resultDefault);
        Assert.Null(resultNull);
        Assert.Equivalent(new List<string> {"1-2-3"}, resultSingle);
    }
}