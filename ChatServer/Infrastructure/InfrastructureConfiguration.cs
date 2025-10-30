namespace Infrastructure;

using Auth;
using Domain.Ports.Auth;
using Domain.Ports.Realtime;
using Domain.Ports.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Realtime;
using Repositories;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
    services.AddDbContext<ChatDatabaseContext>(options => options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
    services.AddSingleton<IPasswordHasher,ArgonPasswordHasher>();
    services.AddSingleton<ITokenGenerator,TokenGenerator>();
    services.AddSingleton<ITokenValidator,JwtTokenValidator>();
    services.AddSingleton<IMessageBroadcaster,MessageBroadcaster>();
    services.AddScoped<IMessageRepository,MessageRepository>();
    services.AddScoped<IChatRepository,ChatRepository>();
    services.AddScoped<IRefreshTokenRepository,RefreshTokenRepository>();
    services.AddScoped<IUserRepository,UserRepository>();
    services.AddScoped<IUnitOfWork,UnitOfWork>();
    
    return services;
  }
}
