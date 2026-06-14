namespace BSDCPolls.Api.Business.UsernameGeneration;

/// <summary>
/// Generates unique, profanity-free system usernames using an adjective-adjective-noun pattern.
/// </summary>
public interface IUsernameGenerator
{
    /// <summary>
    /// Generates a unique username in <c>adj-adj-noun</c> format.
    /// Retries up to 10 times to avoid profanity and collision with <paramref name="existsAsync"/>.
    /// </summary>
    /// <param name="existsAsync">Delegate that returns <c>true</c> if the candidate username is already taken.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A unique, safe username string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts are exhausted.</exception>
    Task<string> GenerateAsync(
        Func<string, Task<bool>> existsAsync,
        CancellationToken ct = default
    );
}
