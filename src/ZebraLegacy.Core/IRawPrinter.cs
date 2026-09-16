namespace ZebraLegacy.Core;

public interface IRawPrinter
{
    void Send(string printerName, ReadOnlySpan<byte> payload, string documentName);
}

public sealed class RawPrintException : Exception
{
    public RawPrintException(string message, Exception? innerException = null) : base(message, innerException) { }
}
