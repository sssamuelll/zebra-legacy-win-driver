using System.Globalization;
using System.Text;

namespace ZebraLegacy.Core;

public static class LabelPayloadGenerator
{
    public static byte[] Generate(PrinterProfile profile, SmokeLabel label)
    {
        var errors = ProfileValidator.Validate(profile);
        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors), nameof(profile));

        return profile.Language switch
        {
            PrinterLanguage.Epl2 => Encoding.ASCII.GetBytes(GenerateEpl2(profile, label)),
            PrinterLanguage.Zpl => Encoding.ASCII.GetBytes(GenerateZpl(profile, label)),
            _ => throw new ArgumentOutOfRangeException(nameof(profile), "Lenguaje no soportado.")
        };
    }

    private static string GenerateEpl2(PrinterProfile profile, SmokeLabel label)
    {
        var lines = new[]
        {
            "N",
            $"q{profile.WidthDots.ToString(CultureInfo.InvariantCulture)}",
            $"Q{profile.HeightDots.ToString(CultureInfo.InvariantCulture)},24",
            $"A30,30,0,4,1,1,N,\"{EplText(label.Title)}\"",
            $"A30,90,0,3,1,1,N,\"Modelo: {EplText(label.Model)}\"",
            $"A30,135,0,2,1,1,N,\"{EplText(label.Timestamp)}\"",
            $"A30,175,0,2,1,1,N,\"Token: {EplText(label.Token)}\"",
            "P1"
        };
        return string.Join('\n', lines) + "\n";
    }

    private static string GenerateZpl(PrinterProfile profile, SmokeLabel label)
    {
        var width = profile.WidthDots.ToString(CultureInfo.InvariantCulture);
        var height = profile.HeightDots.ToString(CultureInfo.InvariantCulture);
        return $"^XA\n^PW{width}\n^LL{height}\n" +
               $"^FO30,30^A0N,34,34^FD{ZplText(label.Title)}^FS\n" +
               $"^FO30,90^A0N,28,28^FDModelo: {ZplText(label.Model)}^FS\n" +
               $"^FO30,135^A0N,24,24^FD{ZplText(label.Timestamp)}^FS\n" +
               $"^FO30,175^A0N,24,24^FDToken: {ZplText(label.Token)}^FS\n^XZ\n";
    }

    private static string EplText(string value) => PrintableAscii(value).Replace("\"", "'", StringComparison.Ordinal);
    private static string ZplText(string value) => PrintableAscii(value).Replace("^", " ", StringComparison.Ordinal).Replace("~", " ", StringComparison.Ordinal);

    private static string PrintableAscii(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "-";
        var chars = value.Trim().Select(character => character is >= ' ' and <= '~' ? character : '?').ToArray();
        return new string(chars);
    }
}
