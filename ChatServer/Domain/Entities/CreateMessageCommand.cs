namespace Domain.Entities;

public record CreateMessageCommand(int ChatId, int SenderId, byte[] AesEncryptedContent);
