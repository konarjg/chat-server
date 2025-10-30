namespace Domain;

using Entities;
using Exceptions;
using Interfaces;
using Ports.Auth;
using Ports.Repositories;

public class AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator) : IAuthService {

  public async Task<AuthResult> RegisterAsync(RegisterUserCommand command) {
    if (await userRepository.GetByNameAsync(command.Name) != null) {
      throw new UserAlreadyExistsException($"User with name {command.Name} already exists.");
    }

    User user = new() {
      Name = command.Name,
      PasswordHash = passwordHasher.HashPassword(command.Password),
      PublicKey = command.PublicKey
    };
    
    await userRepository.AddAsync(user);
    
    return await Authenticate(user);
  }

  public async Task<AuthResult> LoginAsync(LoginUserCommand command) {
    User? user = await userRepository.GetByNameAsync(command.Name);
    
    if (user == null) {
      throw new UserNotFoundException($"User with name {command.Name} does not exist.");
    }

    if (!passwordHasher.VerifyPassword(command.Password,user.PasswordHash)) {
      throw new InvalidPasswordException($"Invalid password.");
    }

    return await Authenticate(user);
  }

  public async Task<AuthResult> RefreshAsync(string refreshToken) {
    RefreshToken? previousRefreshToken =  await RevokeExistingRefreshToken(refreshToken);
    User? user = await userRepository.GetByIdAsync(previousRefreshToken.UserId);

    if (user == null) {
      throw new UserNotFoundException($"User with id {previousRefreshToken.UserId} does not exist.");
    }

    return await Authenticate(user);
  }

  public async Task LogoutAsync(string refreshToken) {
    await RevokeExistingRefreshToken(refreshToken);
    await unitOfWork.CompleteAsync();
  }

  private async Task<RefreshToken> RevokeExistingRefreshToken(string refreshToken) {
    RefreshToken? existingRefreshToken = await refreshTokenRepository.GetByTokenAsync(refreshToken);
    
    if (existingRefreshToken == null || existingRefreshToken.Expires < DateTime.Now || existingRefreshToken.Revoked != null) {
      throw new InvalidRefreshTokenException("Invalid refresh token.");
    }
    
    existingRefreshToken.Revoked = DateTime.UtcNow;
    return existingRefreshToken;
  }

  private async Task<AuthResult> Authenticate(User user) {
    string accessToken = tokenGenerator.GenerateAccessToken(user);
    RefreshToken refreshToken = tokenGenerator.GenerateRefreshToken(user);
    refreshToken.User = user;

    await refreshTokenRepository.AddAsync(refreshToken);
    await unitOfWork.CompleteAsync();
    
    return new AuthResult(accessToken, refreshToken);
  }
}
