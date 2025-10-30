namespace ChatServer.Services;

using Chat;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Grpc.Core;
using Mappers;

public class AuthServiceImpl(IAuthService authService) : AuthService.AuthServiceBase {

  public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context) {
    try {
      RegisterUserCommand command = AuthMapper.ToRegisterCommand(request);
      AuthResult result = await authService.RegisterAsync(command);
      return AuthMapper.ToResponse(result);
    }
    catch (UserAlreadyExistsException ex) {
      throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
    }
  }

  public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context) {
    try {
      LoginUserCommand command = AuthMapper.ToLoginCommand(request);
      AuthResult result = await authService.LoginAsync(command);
      return AuthMapper.ToResponse(result);
    }
    catch (UserNotFoundException ex) {
      throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
    }
    catch (InvalidPasswordException ex) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
    }
  }

  public override async Task<AuthResponse> Refresh(RefreshRequest request, ServerCallContext context) {
    try {
      AuthResult result = await authService.RefreshAsync(request.RefreshToken);
      return AuthMapper.ToResponse(result);
    }
    catch (UserNotFoundException ex) {
      throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
    }
    catch (InvalidRefreshTokenException ex) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
    }
  }

  public override async Task<LogoutResponse> Logout(LogoutRequest request, ServerCallContext context) {
    try {
      await authService.LogoutAsync(request.RefreshToken);
      return new LogoutResponse() {
        Message = "Logout successful."
      };
    }
    catch (InvalidRefreshTokenException ex) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, ex.Message));
    }
  }
}