namespace ChatServer;

using Domain;
using Domain.Interfaces;
using Domain.Ports.Realtime;

public static class DomainConfiguration {
  public static IServiceCollection AddDomain(this IServiceCollection services) {
    services.AddScoped<IChatService,ChatService>();
    services.AddScoped<IMessageService,MessageService>();
    services.AddScoped<IAuthService,AuthService>();
    services.AddScoped<IUserService,UserService>();
    services.AddScoped<IMessageStream,MessageStream>();
    
    return services;
  }
}
