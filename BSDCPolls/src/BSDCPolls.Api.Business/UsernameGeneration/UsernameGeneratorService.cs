using System.Reflection;

namespace BSDCPolls.Api.Business.UsernameGeneration;

/// <summary>
/// Generates <c>adj-adj-noun</c> usernames from embedded word lists.
/// Filters against a profanity deny-list and a caller-supplied uniqueness check.
/// Registered as <c>AddSingleton</c> — all state is read-only after construction.
/// </summary>
public sealed class UsernameGeneratorService : IUsernameGenerator
{
    private const int MaxRetries = 10;

    private readonly string[] _adjectives;
    private readonly string[] _nouns;
    private readonly HashSet<string> _profanity;
    private readonly Random _rng = new();

    /// <summary>Loads word lists from embedded resources at startup.</summary>
    public UsernameGeneratorService()
    {
        var assembly = Assembly.GetExecutingAssembly();
        _adjectives = LoadEmbeddedLines(assembly, "adjectives.txt");
        _nouns = LoadEmbeddedLines(assembly, "nouns.txt");
        _profanity = new HashSet<string>(
            LoadEmbeddedLines(assembly, "profanity.txt"),
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <inheritdoc />
    public async Task<string> GenerateAsync(
        Func<string, Task<bool>> existsAsync,
        CancellationToken ct = default
    )
    {
        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            var candidate = BuildCandidate();

            if (ContainsProfanity(candidate))
            {
                continue;
            }

            if (!await existsAsync(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException(
            $"Username generation failed after {MaxRetries} attempts. "
                + "This is extremely unlikely — check word list sizes and uniqueness constraints."
        );
    }

    private string BuildCandidate()
    {
        var adj1 = _adjectives[_rng.Next(_adjectives.Length)];
        var adj2 = _adjectives[_rng.Next(_adjectives.Length)];
        var noun = _nouns[_rng.Next(_nouns.Length)];
        return $"{adj1}-{adj2}-{noun}";
    }

    private bool ContainsProfanity(string candidate) =>
        candidate.Split('-').Any(word => _profanity.Contains(word));

    private static string[] LoadEmbeddedLines(Assembly assembly, string fileName)
    {
        var resourceName =
            assembly
                .GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"Embedded resource '{fileName}' not found in {assembly.GetName().Name}."
            );

        using var stream =
            assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Could not open embedded resource '{resourceName}'."
            );

        using var reader = new StreamReader(stream);
        return reader
            .ReadToEnd()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !line.StartsWith('#'))
            .ToArray();
    }
}
