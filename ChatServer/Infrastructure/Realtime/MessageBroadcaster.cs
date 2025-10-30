namespace Infrastructure.Realtime;

using System.Collections.Concurrent;
using Domain.Entities;
using Domain.Ports.Realtime;
using Domain.Ports.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class MessageBroadcaster(
  IServiceScopeFactory scopeFactory,
  ILogger<MessageBroadcaster> logger) : IMessageBroadcaster {
  
  private readonly ConcurrentDictionary<int, MessageReceivedCallback> _activeSessions = new();

  public Task ConnectUserAsync(int userId, MessageReceivedCallback messageReceivedCallback) {
    _activeSessions[userId] = messageReceivedCallback;
    logger.LogInformation("User connected to broadcaster: {UserId}", userId);
    return Task.CompletedTask;
  }

  public Task DisconnectUserAsync(int userId) {
    if (_activeSessions.TryRemove(userId, out _)) {
      logger.LogInformation("User disconnected from broadcaster: {UserId}", userId);
    }
    return Task.CompletedTask;
  }

  public async Task BroadcastMessageAsync(Message message) {
    using (IServiceScope scope = scopeFactory.CreateScope()) {
      IChatRepository chatRepository = scope.ServiceProvider.GetRequiredService<IChatRepository>();

      Chat? chat = await chatRepository.GetByIdAsync(message.ChatId);
      if (chat is null) {
        return;
      }

      int recipientId = chat.SenderId == message.SenderId ? chat.ReceiverId : chat.SenderId;

      if (_activeSessions.TryGetValue(recipientId, out MessageReceivedCallback? callback)) {
        try {
          await callback(message);
          logger.LogInformation("Broadcasted message {MessageId} to user {RecipientId}", message.Id, recipientId);
        }
        catch (Exception ex) {
          logger.LogWarning(ex, "Failed to broadcast to user {RecipientId}. Removing stale connection.", recipientId);
          await DisconnectUserAsync(recipientId);
        }
      }
    }
  }
}
