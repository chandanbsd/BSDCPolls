using System.ComponentModel.DataAnnotations;
using BSDCPolls.Contracts.Documents;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Payload for creating a new survey with its initial question tree.</summary>
/// <param name="Title">Survey title; 1–200 characters.</param>
/// <param name="IsPublic">Whether the survey appears in all users' feeds.</param>
/// <param name="QuestionTree">Initial conditional question tree.</param>
public sealed record CreateSurveyRequest(
    [Required] [MaxLength(200)] string Title,
    bool IsPublic,
    [Required] SurveyQuestionTreeDocument QuestionTree);
