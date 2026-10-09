using LogHub.Configuration;

namespace LogHub.Tests.Configuration;
[TestClass]
[DoNotParallelize]
public class ClassicConfigServiceTests
{
    [TestMethod]
    public void A_ShouldStartWithEmptyState()
    {
        var service = ClassicConfigService.Instance();
        Assert.IsNull(service.GetValue("mode"));
        service.SetValue("mode", "A");
    }
    [TestMethod]
    [Ignore("Демонстрация переноса состояния между тестами из-за глобального состояния классического Singleton")]
    public void B_ShouldStartWithEmptyState()
    {
        var service = ClassicConfigService.Instance();
        Assert.IsNull(service.GetValue("mode"));
        service.SetValue("mode", "B");
    }
}
[TestClass]
public class LazyConfigServiceTests
{
    [TestMethod]
    public void Instance_ShouldReturnSameObject()
    {
        var first = LazyConfigService.Instance();
        var second = LazyConfigService.Instance();
        Assert.AreSame(first, second);
    }
}
[TestClass]
public class ConfigServiceTests
{
    [TestMethod]
    public void DifferentInstances_ShouldHaveIndependentState()
    {
        var first = new ConfigService(new LogConfiguration());
        var second = new ConfigService(new LogConfiguration());
        first.SetValue("mode", "first");
        Assert.AreEqual("first", first.GetValue("mode"));
        Assert.IsNull(second.GetValue("mode"));
    }
}