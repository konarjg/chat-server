namespace Infrastructure.Repositories;

using Domain.Entities;
using Domain.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenRepository(ChatDatabaseContext databaseContext) : IRefreshTokenRepository{

  public async Task<RefreshToken?> GetByTokenAsync(string token) {
    return await databaseContext.RefreshTokens.Where(r => r.Token == token).FirstOrDefaultAsync();
  }

  public async Task AddAsync(RefreshToken token) {
    await databaseContext.RefreshTokens.AddAsync(token);
  }
}
