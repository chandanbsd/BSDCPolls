namespace BSDCPolls.Contracts.Responses;

/// <summary>
/// A single field-level validation error within an <see cref="ApiErrorResponse"/>.
/// </summary>
/// <param name="Field">The request field that failed validation; null for object-level errors.</param>
/// <param name="Message">A user-readable description of the validation failure.</param>
public sealed record ApiFieldError(string? Field, string Message);
