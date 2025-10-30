namespace Domain.Entities;

public record class User {
  public int Id { get; init; }
  public required string Name { get; init; }
  public required string PasswordHash { get; init; }
  public required string PublicKey { get; init; }
}
