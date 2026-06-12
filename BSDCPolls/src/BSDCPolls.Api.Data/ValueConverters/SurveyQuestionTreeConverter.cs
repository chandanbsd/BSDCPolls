using System.Text.Json;
using BSDCPolls.Contracts.Documents;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BSDCPolls.Api.Data.ValueConverters;

/// <summary>
/// EF Core value converter that serialises/deserialises <see cref="SurveyQuestionTreeDocument"/>
/// to/from a JSON string for storage as a PostgreSQL JSONB column.
/// </summary>
public sealed class SurveyQuestionTreeConverter
    : ValueConverter<SurveyQuestionTreeDocument, string>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>Initialises a new instance of <see cref="SurveyQuestionTreeConverter"/>.</summary>
    public SurveyQuestionTreeConverter()
        : base(
            v => JsonSerializer.Serialize(v, SerializerOptions),
            v => JsonSerializer.Deserialize<SurveyQuestionTreeDocument>(v, SerializerOptions)
                 ?? new SurveyQuestionTreeDocument(new List<SurveyQuestionNode>()))
    {
    }
}
