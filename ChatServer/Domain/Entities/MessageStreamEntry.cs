namespace Domain.Entities;

public record MessageStreamEntry(int ChatId, byte[] AesEncryptedContent);