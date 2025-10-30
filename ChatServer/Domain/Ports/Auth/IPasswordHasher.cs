namespace Domain.Ports.Auth;

public interface IPasswordHasher {
  bool VerifyPassword(string password,
    string hash);
  
  string HashPassword(string password);
}
