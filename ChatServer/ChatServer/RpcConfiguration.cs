namespace ChatServer;

using Services;

public static class RpcConfiguration {
  public static IServiceCollection AddRpcs(this IServiceCollection services,
    IConfiguration configuration) {
    
    services.AddGrpc(options => {
      options.Interceptors.Add<AuthInterceptor>();
    });
    
    services.AddScoped<AuthInterceptor>();
    services.AddScoped<AuthServiceImpl>();
    services.AddScoped<ChatServiceImpl>();
    services.AddScoped<UserServiceImpl>();
    
    return services;
  }

  public static WebApplication MapRpcs(this WebApplication app) {
    app.MapGrpcService<AuthServiceImpl>();
    app.MapGrpcService<ChatServiceImpl>();
    app.MapGrpcService<UserServiceImpl>();
    
    return app;
  }
}
