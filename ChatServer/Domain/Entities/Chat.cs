namespace Domain.Entities;

public record class Chat {
  public int Id { get; init; }
  public required int SenderId { get; init; }
  public required int ReceiverId { get; init; }
  public required byte[] ReceiverEncryptedAesKey { get; init; }
  public required byte[] SenderEncryptedAesKey { get; init; }
  
  public User Sender { get; init; }
  public User Receiver { get; init; }
}
