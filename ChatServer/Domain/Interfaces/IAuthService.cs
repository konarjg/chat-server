namespace Domain.Interfaces;

using Entities;

public interface IAuthService {
  Task<AuthResult> RegisterAsync(RegisterUserCommand command);
  Task<AuthResult> LoginAsync(LoginUserCommand command);
  Task<AuthResult> RefreshAsync(string refreshToken);
  Task LogoutAsync(string refreshToken);
}
