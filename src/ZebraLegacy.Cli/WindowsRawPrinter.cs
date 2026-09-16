using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ZebraLegacy.Core;

namespace ZebraLegacy.Cli;

[SupportedOSPlatform("windows")]
internal sealed class WindowsRawPrinter : IRawPrinter
{
    public void Send(string printerName, ReadOnlySpan<byte> payload, string documentName)
    {
        if (string.IsNullOrWhiteSpace(printerName)) throw new ArgumentException("Printer name is empty.", nameof(printerName));
        if (payload.IsEmpty) throw new ArgumentException("The RAW payload is empty.", nameof(payload));
        if (payload.Length > 1_048_576) throw new ArgumentException("The RAW payload exceeds 1 MiB.", nameof(payload));

        IntPtr printer = IntPtr.Zero;
        var documentStarted = false;
        var completed = false;
        try
        {
            if (!NativeMethods.OpenPrinter(printerName, out printer, IntPtr.Zero))
                ThrowSpooler("Could not open the printer queue");

            var info = new NativeMethods.DocInfo
            {
                DocumentName = SafeDocumentName(documentName),
                DataType = "RAW"
            };
            if (NativeMethods.StartDocPrinter(printer, 1, ref info) == 0)
                ThrowSpooler("Could not start the RAW document");
            documentStarted = true;

            if (!NativeMethods.StartPagePrinter(printer))
                ThrowSpooler("Could not start the page");

            var bytes = payload.ToArray();
            if (!NativeMethods.WritePrinter(printer, bytes, bytes.Length, out var written) || written != bytes.Length)
                ThrowSpooler("The spooler did not accept all bytes");
            if (!NativeMethods.EndPagePrinter(printer))
                ThrowSpooler("Could not end the page");
            if (!NativeMethods.EndDocPrinter(printer))
                ThrowSpooler("Could not commit the RAW document");

            completed = true;
            documentStarted = false;
        }
        catch (RawPrintException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new RawPrintException("RAW submission to the spooler failed. The label contents were not logged.", exception);
        }
        finally
        {
            if (documentStarted && !completed) NativeMethods.AbortPrinter(printer);
            if (printer != IntPtr.Zero) NativeMethods.ClosePrinter(printer);
        }
    }

    private static string SafeDocumentName(string value)
    {
        var safe = new string((value ?? string.Empty).Where(c => !char.IsControl(c)).Take(80).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "Zebra legacy smoke test" : safe;
    }

    private static void ThrowSpooler(string operation)
    {
        var code = Marshal.GetLastWin32Error();
        throw new RawPrintException($"{operation} (Win32 {code}).", new Win32Exception(code));
    }

    private static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DocInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string DocumentName;
            [MarshalAs(UnmanagedType.LPWStr)] public string? OutputFile;
            [MarshalAs(UnmanagedType.LPWStr)] public string DataType;
        }

        [DllImport("winspool.drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenPrinter(string printerName, out IntPtr printer, IntPtr defaults);

        [DllImport("winspool.drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int StartDocPrinter(IntPtr printer, int level, ref DocInfo info);

        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool StartPagePrinter(IntPtr printer);

        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WritePrinter(IntPtr printer, byte[] bytes, int count, out int written);

        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EndPagePrinter(IntPtr printer);

        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EndDocPrinter(IntPtr printer);

        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AbortPrinter(IntPtr printer);

        [DllImport("winspool.drv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ClosePrinter(IntPtr printer);
    }
}
