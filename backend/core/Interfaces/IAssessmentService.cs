using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface IAssessmentService
{
    Task<IEnumerable<Assessment>> GetUserAssessmentsAsync(int userId);
    Task<Assessment> SubmitAssessmentAsync(int userId, int questionnaireId, Dictionary<string, object> answers);
}
