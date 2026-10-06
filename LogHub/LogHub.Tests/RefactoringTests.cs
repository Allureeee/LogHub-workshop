using System.Collections.Generic;
using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Models;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Reading;
using LogHub.Sinks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogHub.Tests;
[TestClass]
public class RefactoringTests
{
    [TestMethod]
    public void Parse_ValidLine_ReturnsLogRecord()
    {
        var parser = new LogParser();
        var result = parser.Parse(
            "2026-01-15 10:00:01|Info|Конфигурация загружена");
        Assert.IsNotNull(result);
        Assert.AreEqual("Info", result.Severity);
        Assert.AreEqual("Конфигурация загружена", result.Message);
    }
    [TestMethod]
    public void Parse_InvalidColumnCount_ReturnsNull()
    {
        var parser = new LogParser();
        var result = parser.Parse("2026-01-15 10:00:01|Info");
        Assert.IsNull(result);
    }
    [TestMethod]
    public void Parse_InvalidDate_ReturnsNull()
    {
        var parser = new LogParser();
        var result = parser.Parse(
            "not-a-date|Error|Не удалось открыть файл");
        Assert.IsNull(result);
    }
    [TestMethod]
    public void Filter_RecordBelowMinimum_IsRejected()
    {
        var filter = new SeverityFilter("Warning");
        var record = new LogRecord
        {
            Severity = "Info",
            Message = "Обычное сообщение"
        };
        Assert.IsFalse(filter.ShouldInclude(record));
    }
    [TestMethod]
    public void Filter_RecordAtMinimum_IsAccepted()
    {
        var filter = new SeverityFilter("Warning");
        var record = new LogRecord
        {
            Severity = "Warning",
            Message = "Предупреждение"
        };
        Assert.IsTrue(filter.ShouldInclude(record));
    }
    [TestMethod]
    public void Processing_RoutesAcceptedRecordsToSink()
    {
        var sink = new FakeSink();
        var processor = new LogProcessingModule(
            new LogFileReader(),
            new LogParser(),
            new SeverityFilter("Info"),
            new LogFormatter("plain"),
            sink);
        var lines = new[]
        {
            "2026-01-15 10:00:01|Debug|Отладочная запись",
            "2026-01-15 10:00:02|Info|Конфигурация загружена",
            "2026-01-15 10:00:03|Error|Не удалось открыть файл"
        };
        var result = processor.Run(lines);
        Assert.AreEqual(2, result.Written);
        Assert.AreEqual(1, result.Skipped);
        Assert.HasCount(2, sink.Records);
        Assert.AreEqual("Info", sink.Records[0].Severity);
        Assert.AreEqual("Error", sink.Records[1].Severity);
    }
    [TestMethod]
    public void Processing_InvalidLine_IsSkipped()
    {
        var sink = new FakeSink();
        var processor = new LogProcessingModule(
            new LogFileReader(),
            new LogParser(),
            new SeverityFilter("Info"),
            new LogFormatter("plain"),
            sink);
        var lines = new[]
        {
            "invalid line",
            "2026-01-15 10:00:01|Info|Конфигурация загружена"
        };
        var result = processor.Run(lines);
        Assert.AreEqual(1, result.Written);
        Assert.AreEqual(1, result.Skipped);
        Assert.HasCount(1, sink.Records);
    }
    [TestMethod]
    public void Processing_SameInputAsGodObject_PreservesCounters()
    {
        var sink = new FakeSink();
        var processor = new LogProcessingModule(
            new LogFileReader(),
            new LogParser(),
            new SeverityFilter("Info"),
            new LogFormatter("plain"),
            sink);
        var lines = new[]
        {
            "2026-01-15 10:00:01|Info|Конфигурация загружена",
            "2026-01-15 10:00:02|Error|Не удалось открыть файл",
            "2026-01-15 10:00:03|Critical|Нет связи с базой данных",
            "2026-01-15 10:00:04|Debug|Отладочная запись",
            "неправильная строка"
        };
        var result = processor.Run(lines);
        Assert.AreEqual(3, result.Written);
        Assert.AreEqual(2, result.Skipped);
        Assert.HasCount(3, sink.Records);
    }
    [TestMethod]
    public void SinkFactory_ConsoleConfiguration_ReturnsConsoleSink()
    {
        var configuration = new LogConfiguration
        {
            Sink = "console"
        };
        using var sink = LogSinkFactory.Create(configuration);
        Assert.IsInstanceOfType<ConsoleLogSink>(sink);
        Assert.AreEqual("console", sink.Name);
    }
    [TestMethod]
    public void Formatter_JsonFormat_ReturnsJsonText()
    {
        var formatter = new LogFormatter("json");
        var record = new LogRecord
        {
            Date = new System.DateTime(2026, 1, 15, 10, 0, 1),
            Severity = "Error",
            Message = "Ошибка"
        };
        var result = formatter.Format(record);
        StringAssert.Contains(result, "\"date\"");
        StringAssert.Contains(result, "\"level\":\"error\"");
        StringAssert.Contains(result, "\"message\":\"Ошибка\"");
    }
    private sealed class FakeSink : ILogSink
    {
        public List<LogRecord> Records { get; } = new();
        public string Name => "fake";
        public void Write(LogRecord record, string text)
        {
            Records.Add(record);
        }
        public void Dispose()
        {
        }
    }
}
