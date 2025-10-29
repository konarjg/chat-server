namespace Domain;

using Entities;
using Interfaces;
using Ports.Realtime;

public class MessageStream(IMessageService messageService, IMessageBroadcaster messageBroadcaster) : IMessageStream {

  public async Task ProcessMessageStreamAsync(int userId,
    IAsyncEnumerable<MessageStreamEntry> incomingMessages,
    MessageReceivedCallback outgoingMessageCallback,
    CancellationToken cancellationToken) {

    await messageBroadcaster.ConnectUserAsync(userId,outgoingMessageCallback);

    try {
      await foreach (MessageStreamEntry entry in incomingMessages.WithCancellation(cancellationToken)) {
        await ProcessMessageAsync(userId,entry.ChatId,entry.AesEncryptedContent);
      }
    } finally {
      await messageBroadcaster.DisconnectUserAsync(userId);
    }
  }

  private async Task ProcessMessageAsync(int senderId,
    int chatId,
    byte[] encryptedContent) {

    CreateMessageCommand createMessageCommand = new(chatId, senderId, encryptedContent);

    Message message = await messageService.CreateMessageAsync(createMessageCommand);
    await messageBroadcaster.BroadcastMessageAsync(message);
  }
}
