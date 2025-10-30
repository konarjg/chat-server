namespace Domain.Ports.Repositories;

using Entities;

public interface IRefreshTokenRepository {
  Task<RefreshToken?> GetByTokenAsync(string token);
  Task AddAsync(RefreshToken token);
}
