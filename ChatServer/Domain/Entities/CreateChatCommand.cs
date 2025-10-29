namespace Domain.Entities;

public record CreateChatCommand(int SenderId, int ReceiverId, byte[] SenderEncryptedAesKey, byte[] ReceiverEncryptedAesKey);
