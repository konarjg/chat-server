namespace ChatServer.Services;

using Chat;
using Grpc.Core;

public class AuthServiceImpl : AuthService.AuthServiceBase {
  public override Task<AuthResponse> Login(LoginRequest request,
    ServerCallContext context) {
    throw new NotImplementedException();
  }
}
