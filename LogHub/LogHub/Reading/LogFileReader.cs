using System.Collections.Generic;
using System.IO;

namespace LogHub.Reading;
public class LogFileReader
{
    public IEnumerable<string> Read(string path)
    {
        return File.ReadLines(path);
    }
}