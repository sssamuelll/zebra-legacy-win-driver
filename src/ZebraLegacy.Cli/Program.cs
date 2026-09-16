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
                _ => Fail("Unknown command. Use --help.")
            };
        }
        catch (JsonException exception)
        {
            return Fail($"Invalid JSON: {exception.Message}");
        }
        catch (IOException exception)
        {
            return Fail($"Could not read the configuration: {exception.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            return Fail("Access denied to the file or printer queue.");
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
        var configPath = Option(args, "--config") ?? throw new ArgumentException("Missing --config <file>.");
        var config = Load(configPath);
        var errors = ProfileValidator.Validate(config);
        if (errors.Count > 0)
        {
            foreach (var error in errors) Console.Error.WriteLine($"ERROR: {error}");
            return 2;
        }
        Console.WriteLine($"Valid configuration: {config.Profiles.Count} profile(s).");
        return 0;
    }

    private static int Smoke(string[] args)
    {
        var configPath = Option(args, "--config") ?? throw new ArgumentException("Missing --config <file>.");
        var profileName = Option(args, "--profile") ?? throw new ArgumentException("Missing --profile <name>.");
        var send = args.Contains("--send", StringComparer.OrdinalIgnoreCase);
        var unknown = args.Where((arg, index) => arg.StartsWith("--", StringComparison.Ordinal) &&
            arg is not "--config" and not "--profile" and not "--send").ToArray();
        if (unknown.Length > 0) throw new ArgumentException($"Unknown option: {unknown[0]}.");

        var config = Load(configPath);
        var configErrors = ProfileValidator.Validate(config);
        if (configErrors.Count > 0) throw new ArgumentException(string.Join(Environment.NewLine, configErrors));
        var profile = config.Profiles.SingleOrDefault(item => string.Equals(item.Name, profileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Profile '{profileName}' does not exist.");

        var now = DateTimeOffset.UtcNow;
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        var label = new SmokeLabel("ZEBRA SMOKE TEST", profile.Model.ToString(), now.ToString("u"), token);
        var payload = LabelPayloadGenerator.Generate(profile, label);
        var digest = Convert.ToHexString(SHA256.HashData(payload));
        Console.WriteLine($"Profile={profile.Name} Language={profile.Language} Bytes={payload.Length} SHA256={digest}");

        if (!send)
        {
            Console.WriteLine("DRY-RUN: nothing was sent. Add --send only after completing the physical plan.");
            return 0;
        }
        if (!OperatingSystem.IsWindows()) return Fail("--send is available only on Windows.");

        new WindowsRawPrinter().Send(profile.PrinterName, payload, $"Zebra smoke {token}");
        Console.WriteLine("The spooler accepted the RAW job. Verify the result physically.");
        return 0;
    }

    private static ProfileConfiguration Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var json = File.ReadAllText(fullPath);
        return JsonSerializer.Deserialize<ProfileConfiguration>(json, JsonOptions)
            ?? throw new ArgumentException("The configuration is empty.");
    }

    private static string? Option(string[] args, string name)
    {
        var index = Array.FindIndex(args, arg => string.Equals(arg, name, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return null;
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"Missing value for {name}.");
        return args[index + 1];
    }

    private static bool IsHelp(string value) => value is "--help" or "-h" or "help";

    private static int Help()
    {
        Console.WriteLine("""
Zebra Legacy CLI (.NET 8)

  validate --config <profiles.json>
  smoke --config <profiles.json> --profile <name> [--send]

'smoke' is a dry-run by default. --send requires Windows and prior physical validation.
""");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine($"ERROR: {message}");
        return 1;
    }
}
