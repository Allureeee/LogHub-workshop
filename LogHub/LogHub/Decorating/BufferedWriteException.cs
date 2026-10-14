namespace LogHub.Decorating;
public sealed class BufferedWriteException : IOException
{
    public BufferedWriteException(IOException inner) : base("The record is buffered, but flushing failed.", inner) { }
}
