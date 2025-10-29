namespace Infrastructure.Repositories;

using Domain.Entities;
using Domain.Ports.Repositories;

public class UserRepository(ChatDatabaseContext databaseContext) : IUserRepository {

  public async Task<User?> GetByIdAsync(int id) {
    return await databaseContext.Users.FindAsync(id);
  }
}
