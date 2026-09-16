using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ZebraLegacy.IntegrationTests;

[TestClass]
[TestCategory("Integration")]
public sealed class PrinterPresenceTests
{
    [TestMethod]
    public void ConfiguredPrinterQueue_CanBeOpenedReadOnly()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Inconclusive("Windows is required.");
        var printer = Environment.GetEnvironmentVariable("ZEBRA_TEST_PRINTER");
        if (string.IsNullOrWhiteSpace(printer))
            Assert.Inconclusive("Set ZEBRA_TEST_PRINTER to an authorized queue.");

        Assert.IsTrue(OpenPrinter(printer, out var handle, IntPtr.Zero),
            $"Could not open the configured queue (Win32 {Marshal.GetLastWin32Error()}).");
        try
        {
            Assert.AreNotEqual(IntPtr.Zero, handle);
        }
        finally
        {
            if (handle != IntPtr.Zero) ClosePrinter(handle);
        }
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenPrinter(string printerName, out IntPtr printer, IntPtr defaults);

    [DllImport("winspool.drv", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClosePrinter(IntPtr printer);
}
