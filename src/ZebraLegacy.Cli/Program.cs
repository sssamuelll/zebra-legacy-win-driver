using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZebraLegacy.Cli;
using ZebraLegacy.Core;

return Cli.Run(args);

internal static class Cli
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static int Run(string[] args)
    {
        try
        {
            if (args.Length == 0 || IsHelp(args[0])) return Help();
            return args[0].ToLowerInvariant() switch
            {
                "validate" => Validate(args[1..]),
                "smoke" => Smoke(args[1..]),
                _ => Fail("Comando desconocido. Use --help.")
            };
        }
        catch (JsonException exception)
        {
            return Fail($"JSON inválido: {exception.Message}");
        }
        catch (IOException exception)
        {
            return Fail($"No se pudo leer la configuración: {exception.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            return Fail("Acceso denegado al archivo o a la cola de impresión.");
        }
        catch (RawPrintException exception)
        {
            return Fail(exception.Message);
        }
        catch (ArgumentException exception)
        {
            return Fail(exception.Message);
        }
    }

    private static int Validate(string[] args)
    {
        var configPath = Option(args, "--config") ?? throw new ArgumentException("Falta --config <archivo>.");
        var config = Load(configPath);
        var errors = ProfileValidator.Validate(config);
        if (errors.Count > 0)
        {
            foreach (var error in errors) Console.Error.WriteLine($"ERROR: {error}");
            return 2;
        }
        Console.WriteLine($"Configuración válida: {config.Profiles.Count} perfil(es).");
        return 0;
    }

    private static int Smoke(string[] args)
    {
        var configPath = Option(args, "--config") ?? throw new ArgumentException("Falta --config <archivo>.");
        var profileName = Option(args, "--profile") ?? throw new ArgumentException("Falta --profile <nombre>.");
        var send = args.Contains("--send", StringComparer.OrdinalIgnoreCase);
        var unknown = args.Where((arg, index) => arg.StartsWith("--", StringComparison.Ordinal) &&
            arg is not "--config" and not "--profile" and not "--send").ToArray();
        if (unknown.Length > 0) throw new ArgumentException($"Opción desconocida: {unknown[0]}.");

        var config = Load(configPath);
        var configErrors = ProfileValidator.Validate(config);
        if (configErrors.Count > 0) throw new ArgumentException(string.Join(Environment.NewLine, configErrors));
        var profile = config.Profiles.SingleOrDefault(item => string.Equals(item.Name, profileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"No existe el perfil '{profileName}'.");

        var now = DateTimeOffset.UtcNow;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        var label = new SmokeLabel("ZEBRA SMOKE TEST", profile.Model.ToString(), now.ToString("u"), token);
        var payload = LabelPayloadGenerator.Generate(profile, label);
        var digest = Convert.ToHexString(SHA256.HashData(payload));
        Console.WriteLine($"Perfil={profile.Name} Lenguaje={profile.Language} Bytes={payload.Length} SHA256={digest}");

        if (!send)
        {
            Console.WriteLine("DRY-RUN: no se ha enviado nada. Añada --send solo tras completar el plan físico.");
            return 0;
        }
        if (!OperatingSystem.IsWindows()) return Fail("--send solo está disponible en Windows.");

        new WindowsRawPrinter().Send(profile.PrinterName, payload, $"Zebra smoke {token}");
        Console.WriteLine("El spooler aceptó el trabajo RAW. Verifique el resultado físicamente.");
        return 0;
    }

    private static ProfileConfiguration Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<ProfileConfiguration>(json, JsonOptions)
            ?? throw new ArgumentException("La configuración está vacía.");
    }

    private static string? Option(string[] args, string name)
    {
        var index = Array.FindIndex(args, arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return null;
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"Falta el valor de {name}.");
        return args[index + 1];
    }

    private static bool IsHelp(string value) => value is "--help" or "-h" or "help";

    private static int Help()
    {
        Console.WriteLine("""
Zebra Legacy CLI (.NET 8)

  validate --config <profiles.json>
  smoke --config <profiles.json> --profile <nombre> [--send]

'smoke' es dry-run por defecto. --send requiere Windows y una validación física previa.
""");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"ERROR: {message}");
        return 1;
    }
}
