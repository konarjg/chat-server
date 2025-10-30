namespace Domain.Ports.Repositories;

using Entities;

public interface IUserRepository {
  Task<User?> GetByIdAsync(int id);
  Task<User?> GetByNameAsync(string name);
  Task<PagedResult<User>> GetPagedAsync(int pageSize,
    int? lastId = null);
  Task AddAsync(User user);
}
