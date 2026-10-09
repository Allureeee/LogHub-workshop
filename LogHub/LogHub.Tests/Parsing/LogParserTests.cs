using LogHub.Parsing;

namespace LogHub.Tests.Parsing;
[TestClass]
public class LogParserVariant20Tests
{
    [TestMethod]
    public void ParseJson_ShouldReturnLogRecord()
    {
        var parser = new LogParser();
        var result = parser.Parse(
            """{"date":"2026-10-07T10:01:00","level":"Warning","message":"Высокая нагрузка"}""");
        Assert.IsNotNull(result);
        Assert.AreEqual("Warning", result.Severity);
        Assert.AreEqual("Высокая нагрузка", result.Message);
        Assert.AreEqual(
            new DateTime(2026, 10, 7, 10, 1, 0),
            result.Date);
    }
    [TestMethod]
    public void ParseMixedFormats_ShouldReturnRecordsForBothFormats()
    {
        var parser = new LogParser();
        var first = parser.Parse(
            "2026-10-07T10:00:00|Info|Система запущена");
        var second = parser.Parse(
            """{"date":"2026-10-07T10:01:00","level":"Error","message":"Ошибка подключения"}""");
        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        Assert.AreEqual("Info", first.Severity);
        Assert.AreEqual("Система запущена", first.Message);
        Assert.AreEqual("Error", second.Severity);
        Assert.AreEqual("Ошибка подключения", second.Message);
    }
    [TestMethod]
    public void InvalidJson_ShouldReturnNull()
    {
        var parser = new LogParser();
        var result = parser.Parse(
            """{"date":"2026-10-07T10:01:00","level":"Error"}""");
        Assert.IsNull(result);
    }
}