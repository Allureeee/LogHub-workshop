namespace LogHub.Tests;

[TestClass]
public class SmokeTests
{
    [TestMethod]
    public void TestProject_IsWiredCorrectly()
    {
        var assembly = typeof(SmokeTests).Assembly;
        Assert.IsNotNull(assembly);
    }
}