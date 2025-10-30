namespace Domain.Ports.Auth;

using Entities;

public interface ITokenGenerator {
  string GenerateAccessToken(User user);
  RefreshToken GenerateRefreshToken(User user);
}
