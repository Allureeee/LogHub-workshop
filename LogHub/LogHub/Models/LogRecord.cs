using System;
namespace LogHub.Models;
public class LogRecord
{
    public DateTime Date { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}