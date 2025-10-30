namespace ChatServer.Services;

using System.Security.Claims;
using Domain.Entities;
using Domain.Ports.Auth;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

public class AuthInterceptor(
  ITokenValidator tokenValidator,
  ILogger<AuthInterceptor> logger) : Interceptor {

  public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
    TRequest request,
    ServerCallContext context,
    UnaryServerMethod<TRequest, TResponse> continuation) {
    
    ValidateAndSetUserPrincipal(context);
    return await continuation(request, context);
  }

  public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
    IAsyncStreamReader<TRequest> requestStream,
    ServerCallContext context,
    ClientStreamingServerMethod<TRequest, TResponse> continuation) {
    
    ValidateAndSetUserPrincipal(context);
    return await continuation(requestStream, context);
  }

  public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
    TRequest request,
    IServerStreamWriter<TResponse> responseStream,
    ServerCallContext context,
    ServerStreamingServerMethod<TRequest, TResponse> continuation) {
    
    ValidateAndSetUserPrincipal(context);
    await continuation(request, responseStream, context);
  }

  public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
    IAsyncStreamReader<TRequest> requestStream,
    IServerStreamWriter<TResponse> responseStream,
    ServerCallContext context,
    DuplexStreamingServerMethod<TRequest, TResponse> continuation) {
    
    ValidateAndSetUserPrincipal(context);
    await continuation(requestStream, responseStream, context);
  }

  private void ValidateAndSetUserPrincipal(ServerCallContext context) {
    if (context.Method.StartsWith("/chat.AuthService")) {
      return;
    }

    HttpContext httpContext = context.GetHttpContext();
    string? authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

    if (authHeader is null || !authHeader.StartsWith("Bearer ")) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Authorization token is required."));
    }

    string token = authHeader.Split(' ').Last();
    AuthenticatedUser? authenticatedUser = tokenValidator.ValidateToken(token);

    if (authenticatedUser is null) {
      logger.LogWarning("Authentication failed for method {Method}: Invalid token.", context.Method);
      throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token."));
    }

    Claim[] claims = [
      new Claim(ClaimTypes.NameIdentifier, authenticatedUser.Id.ToString()),
      new Claim(ClaimTypes.Name, authenticatedUser.Name),
    ];
    
    ClaimsIdentity identity = new(claims, "jwt");
    httpContext.User = new ClaimsPrincipal(identity);
  }
}
