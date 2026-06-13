using System.ComponentModel.DataAnnotations;
using BSDCPolls.Contracts.Documents;

namespace BSDCPolls.Contracts.Requests.Surveys;

/// <summary>Payload for replacing a draft survey's entire question tree.</summary>
/// <param name="QuestionTree">The new conditional question tree to store.</param>
public sealed record UpdateSurveyQuestionsRequest([Required] SurveyQuestionTreeDocument QuestionTree);
