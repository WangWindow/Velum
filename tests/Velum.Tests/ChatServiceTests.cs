using Microsoft.EntityFrameworkCore;
using Moq;
using Velum.Base.Data;
using Velum.Base.Services;
using Velum.Core.Interfaces;
using Velum.Core.Models;
using Xunit;

namespace Velum.Tests;

public class ChatServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IOpenAIService> _mockOpenAIService;
    private readonly ChatService _chatService;

    public ChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockOpenAIService = new Mock<IOpenAIService>();
        _chatService = new ChatService(_context, _mockOpenAIService.Object);
    }

    [Fact]
    public async Task CreateSessionAsync_ShouldCreateNewSession()
    {
        // Arrange
        var userId = 1;
        var title = "Test Session";

        // Act
        var session = await _chatService.CreateSessionAsync(userId, title);

        // Assert
        Assert.NotNull(session);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(title, session.Title);
        Assert.NotEqual(0, session.Id);

        var dbSession = await _context.ChatSessions.FindAsync(session.Id);
        Assert.NotNull(dbSession);
    }

    [Fact]
    public async Task ProcessUserMessageAsync_ShouldSaveMessagesAndReturnAIResponse()
    {
        // Arrange
        var userId = 1;
        var userMessage = "Hello AI";
        var aiResponse = "Hello User";
        var session = await _chatService.CreateSessionAsync(userId);

        _mockOpenAIService.Setup(s => s.GetChatCompletionAsync(userMessage))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _chatService.ProcessUserMessageAsync(userId, userMessage, session.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(aiResponse, result.Content);
        Assert.Equal(session.Id, result.SessionId);

        var messages = await _context.ChatMessages.Where(m => m.SessionId == session.Id).ToListAsync();
        Assert.Equal(2, messages.Count);
        Assert.Contains(messages, m => m.Role == Core.Enums.ChatRoleType.User && m.Content == userMessage);
        Assert.Contains(messages, m => m.Role == Core.Enums.ChatRoleType.Assistant && m.Content == aiResponse);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ShouldReturnUserSessions()
    {
        // Arrange
        var userId = 1;
        await _chatService.CreateSessionAsync(userId, "Session 1");
        await _chatService.CreateSessionAsync(userId, "Session 2");
        await _chatService.CreateSessionAsync(2, "Other User Session");

        // Act
        var sessions = await _chatService.GetUserSessionsAsync(userId);

        // Assert
        Assert.Equal(2, sessions.Count());
    }

    [Fact]
    public async Task DeleteSessionAsync_ShouldRemoveSessionAndMessages()
    {
        // Arrange
        var userId = 1;
        var session = await _chatService.CreateSessionAsync(userId);

        _mockOpenAIService.Setup(s => s.GetChatCompletionAsync(It.IsAny<string>()))
            .ReturnsAsync("AI Response");

        await _chatService.ProcessUserMessageAsync(userId, "Hi", session.Id);

        // Act
        await _chatService.DeleteSessionAsync(session.Id, userId);

        // Assert
        var dbSession = await _context.ChatSessions.FindAsync(session.Id);
        Assert.Null(dbSession);

        var messages = await _context.ChatMessages.Where(m => m.SessionId == session.Id).ToListAsync();
        Assert.Empty(messages);
    }
}
