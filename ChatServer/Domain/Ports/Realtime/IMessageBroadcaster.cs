namespace Domain.Ports.Realtime;

using Entities;

public interface IMessageBroadcaster {
  Task ConnectUserAsync(int userId, MessageReceivedCallback messageReceivedCallback);
  Task DisconnectUserAsync(int userId);
  Task BroadcastMessageAsync(Message message);
}
