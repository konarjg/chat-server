namespace Domain.Ports.Repositories;

using Entities;

public interface IUserRepository {
  Task<User?> GetByIdAsync(int id);
}
