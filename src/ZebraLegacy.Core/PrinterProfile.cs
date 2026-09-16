namespace ZebraLegacy.Core;

public enum PrinterModel
{
    Gc420d,
    Tlp2844,
    Tlp2844Z
}

public enum PrinterLanguage
{
    Epl2,
    Zpl
}

public sealed record PrinterProfile(
    string Name,
    string PrinterName,
    PrinterModel Model,
    PrinterLanguage Language,
    int Dpi,
    int WidthDots,
    int HeightDots);

public sealed record ProfileConfiguration(IReadOnlyList<PrinterProfile> Profiles);

public sealed record SmokeLabel(string Title, string Model, string Timestamp, string Token);
