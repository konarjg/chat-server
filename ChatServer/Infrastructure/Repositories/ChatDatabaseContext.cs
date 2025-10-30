namespace Infrastructure.Repositories;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class ChatDatabaseContext : DbContext {
  public DbSet<User> Users { get; init; }
  public DbSet<Chat> Chats { get; init; }
  public DbSet<Message> Messages { get; init; }
  public DbSet<RefreshToken> RefreshTokens { get; init; }
  
  public ChatDatabaseContext(DbContextOptions<ChatDatabaseContext> options) : base(options) {}
}
