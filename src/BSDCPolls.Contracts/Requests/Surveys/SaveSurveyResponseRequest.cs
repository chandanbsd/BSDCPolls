using System.ComponentModel.DataAnnotations;
using BSDCPolls.Contracts.Documents;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Payload for saving progress or submitting a survey response.</summary>
/// <param name="Answers">All answered question entries so far.</param>
/// <param name="IsSubmitting">When true, finalises the response and prevents further edits.</param>
public sealed record SaveSurveyResponseRequest(
    [Required] IReadOnlyList<SurveyAnswerEntry> Answers,
    bool IsSubmitting
);
