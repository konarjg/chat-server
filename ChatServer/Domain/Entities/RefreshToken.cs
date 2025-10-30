namespace Domain.Entities;

public class RefreshToken {
  public int Id { get; set; }
  public required int UserId { get; set; }
  public required string Token { get; set; }
  public required DateTime Expires { get; set; }
  public DateTime? Revoked { get; set; }
  
  public User User { get; set; }
}
