using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZebraLegacy.Core;

namespace ZebraLegacy.Core.Tests;

[TestClass]
public sealed class ProfileValidatorTests
{
    [TestMethod]
    public void ValidProfile_HasNoErrors()
    {
        var profile = new PrinterProfile("gc420d-01", "ZDesigner GC420d", PrinterModel.Gc420d, PrinterLanguage.Zpl, 203, 812, 406);
        Assert.AreEqual(0, ProfileValidator.Validate(profile).Count);
    }

    [TestMethod]
    public void Tlp2844WithoutZ_RejectsZpl()
    {
        var profile = new PrinterProfile("tlp", "ZDesigner TLP 2844", PrinterModel.Tlp2844, PrinterLanguage.Zpl, 203, 812, 406);
        Assert.IsTrue(ProfileValidator.Validate(profile).Any(error => error.Contains("EPL2", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Configuration_RejectsDuplicateNamesCaseInsensitively()
    {
        var first = new PrinterProfile("warehouse", "ZDesigner GC420d", PrinterModel.Gc420d, PrinterLanguage.Epl2, 203, 812, 406);
        var second = first with { Name = "WAREHOUSE" };
        var errors = ProfileValidator.Validate(new ProfileConfiguration(new[] { first, second }));
        Assert.IsTrue(errors.Any(error => error.Contains("Duplicate", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void InvalidDimensionsAndDriver_AreReported()
    {
        var profile = new PrinterProfile("bad name", "Generic Text", PrinterModel.Gc420d, PrinterLanguage.Epl2, 300, 10, 99);
        Assert.IsTrue(ProfileValidator.Validate(profile).Count >= 5);
    }
}
