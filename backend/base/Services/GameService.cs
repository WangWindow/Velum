using Microsoft.EntityFrameworkCore;
using Velum.Base.Data;
using Velum.Core.Interfaces;
using Velum.Core.Models;

namespace Velum.Base.Services;

public class GameService(ApplicationDbContext context) : IGameService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<GameScore> SubmitScoreAsync(int userId, string gameName, int score, double duration)
    {
        var gameScore = new GameScore
        {
            UserId = userId,
            GameName = gameName,
            Score = score,
            Duration = duration,
            PlayedAt = DateTime.UtcNow
        };

        _context.GameScores.Add(gameScore);
        await _context.SaveChangesAsync();
        return gameScore;
    }

    public async Task<IEnumerable<GameScore>> GetUserScoresAsync(int userId)
    {
        return await _context.GameScores
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.PlayedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameScore>> GetTopScoresAsync(string gameName, int count = 10)
    {
        return await _context.GameScores
            .Include(s => s.User)
            .Where(s => s.GameName == gameName)
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameScore>> GetAllScoresAsync()
    {
        return await _context.GameScores
            .Include(s => s.User)
            .OrderByDescending(s => s.PlayedAt)
            .ToListAsync();
    }
}
