namespace Domain.Entities;

public record class Message {
  public int Id { get; init; }
  public required int ChatId { get; init; }
  public required int SenderId { get; init; }
  public required byte[] AesEncryptedContent { get; init; }
  
  public Chat Chat { get; init; }
}
