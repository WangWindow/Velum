using Velum.Core.Models;

namespace Velum.Core.Interfaces;

public interface IGameService
{
    Task<GameScore> SubmitScoreAsync(int userId, string gameName, int score, double duration);
    Task<IEnumerable<GameScore>> GetUserScoresAsync(int userId);
    Task<IEnumerable<GameScore>> GetTopScoresAsync(string gameName, int count = 10);
    Task<IEnumerable<GameScore>> GetAllScoresAsync();
}
