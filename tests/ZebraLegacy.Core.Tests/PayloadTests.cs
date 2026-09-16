using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZebraLegacy.Core;

namespace ZebraLegacy.Core.Tests;

[TestClass]
public sealed class PayloadTests
{
    [TestMethod]
    public void Epl2Payload_HasExpectedFramingAndAsciiSanitization()
    {
        var profile = Profile(PrinterModel.Tlp2844, PrinterLanguage.Epl2);
        var bytes = LabelPayloadGenerator.Generate(profile, new SmokeLabel("TEST \"OK\"", "TLP 2844", "2026-09-16", "A^~✓"));
        var payload = Encoding.ASCII.GetString(bytes);

        Assert.IsTrue(payload.StartsWith("N\nq812\nQ406,24\n", StringComparison.Ordinal));
        StringAssert.Contains(payload, "TEST 'OK'");
        StringAssert.Contains(payload, "Model: TLP 2844");
        StringAssert.Contains(payload, "Token: A^~?");
        Assert.IsTrue(payload.EndsWith("P1\n", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ZplPayload_HasExpectedFramingAndNeutralizesCommandMarkers()
    {
        var profile = Profile(PrinterModel.Gc420d, PrinterLanguage.Zpl);
        var bytes = LabelPayloadGenerator.Generate(profile, new SmokeLabel("^XA~JA", "GC420d", "2026-09-16", "ABC"));
        var payload = Encoding.ASCII.GetString(bytes);

        Assert.IsTrue(payload.StartsWith("^XA\n^PW812\n^LL406\n", StringComparison.Ordinal));
        StringAssert.Contains(payload, "^FD XA JA^FS");
        StringAssert.Contains(payload, "^FDModel: GC420d^FS");
        Assert.IsTrue(payload.EndsWith("^XZ\n", StringComparison.Ordinal));
    }

    private static PrinterProfile Profile(PrinterModel model, PrinterLanguage language) =>
        new("test", $"ZDesigner {model}", model, language, 203, 812, 406);
}
