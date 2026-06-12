using System.Text.Json;
using BSDCPolls.Contracts.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BSDCPolls.Api.Data.ValueConverters;

/// <summary>
/// EF Core value converter that serialises/deserialises <see cref="SurveyAnswersDocument"/>
/// to/from a JSON string for storage as a PostgreSQL JSONB column.
/// </summary>
public sealed class SurveyAnswersConverter
    : ValueConverter<SurveyAnswersDocument, string>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>Initialises a new instance of <see cref="SurveyAnswersConverter"/>.</summary>
    public SurveyAnswersConverter()
        : base(
            v => JsonSerializer.Serialize(v, SerializerOptions),
            v => JsonSerializer.Deserialize<SurveyAnswersDocument>(v, SerializerOptions)
                 ?? new SurveyAnswersDocument(new List<SurveyAnswerEntry>()))
    {
    }
}
