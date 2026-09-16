using System.Text.RegularExpressions;

namespace ZebraLegacy.Core;

public static partial class ProfileValidator
{
    public static IReadOnlyList<string> Validate(PrinterProfile? profile)
    {
        var errors = new List<string>();
        if (profile is null)
        {
            errors.Add("El perfil no puede ser nulo.");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(profile.Name) || !ProfileNameRegex().IsMatch(profile.Name))
            errors.Add("Name debe contener solo letras, números, '.', '_' o '-' (1-64 caracteres).");

        if (string.IsNullOrWhiteSpace(profile.PrinterName) || profile.PrinterName.Length > 255 || HasControlCharacters(profile.PrinterName))
            errors.Add("PrinterName debe tener entre 1 y 255 caracteres y no contener controles.");
        else if (!profile.PrinterName.Contains("ZDesigner", StringComparison.OrdinalIgnoreCase))
            errors.Add("PrinterName debe identificar una cola instalada con ZDesigner.");

        if (profile.Dpi != 203)
            errors.Add("Los modelos cubiertos por este proyecto requieren un perfil de 203 dpi.");
        if (profile.WidthDots is < 100 or > 832)
            errors.Add("WidthDots debe estar entre 100 y 832.");
        if (profile.HeightDots is < 100 or > 4000)
            errors.Add("HeightDots debe estar entre 100 y 4000.");
        if (profile.Model == PrinterModel.Tlp2844 && profile.Language != PrinterLanguage.Epl2)
            errors.Add("TLP 2844 (sin sufijo -Z) se configura aquí solo con EPL2; confirme físicamente el modelo.");

        return errors;
    }

    public static IReadOnlyList<string> Validate(ProfileConfiguration? configuration)
    {
        var errors = new List<string>();
        if (configuration?.Profiles is null || configuration.Profiles.Count == 0)
        {
            errors.Add("La configuración debe contener al menos un perfil.");
            return errors;
        }

        for (var index = 0; index < configuration.Profiles.Count; index++)
            errors.AddRange(Validate(configuration.Profiles[index]).Select(error => $"profiles[{index}]: {error}"));

        foreach (var duplicate in configuration.Profiles.Where(p => p is not null).GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            errors.Add($"Nombre de perfil duplicado: {duplicate.Key}.");

        return errors;
    }

    private static bool HasControlCharacters(string value) => value.Any(char.IsControl);

    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex ProfileNameRegex();
}
