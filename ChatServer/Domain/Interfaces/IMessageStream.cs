namespace Domain.Interfaces;

using Entities;

public interface IMessageStream {
  Task ProcessMessageStreamAsync(int userId,
    IAsyncEnumerable<MessageStreamEntry> incomingMessages,
    MessageReceivedCallback outgoingMessageCallback,
    CancellationToken cancellationToken);
}
