namespace ChatServer.Mappers;

using Chat;
using Domain.Entities;
using UserDto = Chat.User;

public static class AuthMapper {
  public static RegisterUserCommand ToRegisterCommand(RegisterRequest registerRequest) {
    return new RegisterUserCommand(registerRequest.Name,registerRequest.Password,registerRequest.PublicKey);
  }

  public static LoginUserCommand ToLoginCommand(LoginRequest loginRequest) {
    return new LoginUserCommand(loginRequest.Name,loginRequest.Password);
  }
  
  public static AuthResponse ToResponse(AuthResult authResult) {
    return new AuthResponse() {
      AccessToken = authResult.AccessToken,
      RefreshToken = authResult.RefreshToken.Token
    };
  }
}
