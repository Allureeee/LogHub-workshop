using LogHub.Configuration;
namespace LogHub.Tests.Configuration;
[TestClass]
public sealed class Lr6ConfigTests
{
    [TestMethod]
    public void ProfileParsing_SeparatesImporterAndOutputFormat()
    {
        var result = new ConfigReader().Parse(new[] { "importer=json", "format=plain", "sink=console,file" },
            Path.GetFullPath("."));
        Assert.AreEqual("json", result.Importer);
        Assert.AreEqual("plain", result.Format);
        Assert.AreEqual("console,file", result.Sink);
    }
    [TestMethod]
    public void ConfigService_ExposesSourceAndOutputPath()
    {
        var service = new ConfigService(new LogConfiguration { Importer = "memory", OutputPath = "output.log" });
        Assert.AreEqual("memory", service.GetValue("importer"));
        Assert.AreEqual("output.log", service.GetValue("outputPath"));
    }
    [TestMethod]
    public void ProfilePaths_AreRelativeToTheProfileDirectory()
    {
        var directory = Path.GetFullPath("profiles");
        var configuration = new ConfigReader().Parse(new[] {
            "logPath=../data/variant20.txt", "outputPath=../output/variant20.log" }, directory);
        Assert.AreEqual(Path.GetFullPath("../data/variant20.txt", directory), configuration.LogPath);
        Assert.AreEqual(Path.GetFullPath("../output/variant20.log", directory), configuration.OutputPath);
    }
}
