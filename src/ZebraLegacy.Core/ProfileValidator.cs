using System.Text.RegularExpressions;

namespace ZebraLegacy.Core;

public static partial class ProfileValidator
{
    public static IReadOnlyList<string> Validate(PrinterProfile? profile)
    {
        var errors = new List<string>();
        if (profile is null)
        {
            errors.Add("The profile cannot be null.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(profile.Name) || !ProfileNameRegex().IsMatch(profile.Name))
            errors.Add("Name must contain only letters, numbers, '.', '_', or '-' (1-64 characters).");

        if (string.IsNullOrWhiteSpace(profile.PrinterName) || profile.PrinterName.Length > 255 || HasControlCharacters(profile.PrinterName))
            errors.Add("PrinterName must be 1 to 255 characters and contain no control characters.");
        else if (!profile.PrinterName.Contains("ZDesigner", StringComparison.OrdinalIgnoreCase))
            errors.Add("PrinterName must identify an installed ZDesigner queue.");

        if (profile.Dpi != 203)
            errors.Add("Models covered by this project require a 203 dpi profile.");
        if (profile.WidthDots is < 100 or > 832)
            errors.Add("WidthDots must be between 100 and 832.");
        if (profile.HeightDots is < 100 or > 4000)
            errors.Add("HeightDots must be between 100 and 4000.");
        if (profile.Model == PrinterModel.Tlp2844 && profile.Language != PrinterLanguage.Epl2)
            errors.Add("TLP 2844 (without the -Z suffix) is configured here only with EPL2; physically confirm the model.");

        return errors;
    }

    public static IReadOnlyList<string> Validate(ProfileConfiguration? configuration)
    {
        var errors = new List<string>();
        if (configuration?.Profiles is null || configuration.Profiles.Count == 0)
        {
            errors.Add("The configuration must contain at least one profile.");
            return errors;
        }

        for (var index = 0; index < configuration.Profiles.Count; index++)
            errors.AddRange(Validate(configuration.Profiles[index]).Select(error => $"profiles[{index}]: {error}"));

        foreach (var duplicate in configuration.Profiles.Where(p => p is not null).GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            errors.Add($"Duplicate profile name: {duplicate.Key}.");

        return errors;
    }

    private static bool HasControlCharacters(string value) => value.Any(char.IsControl);

    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex ProfileNameRegex();
}
