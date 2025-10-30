namespace Domain.Ports.Auth;

using Entities;

public interface ITokenValidator {
  AuthenticatedUser? ValidateToken(string accessToken);
}
