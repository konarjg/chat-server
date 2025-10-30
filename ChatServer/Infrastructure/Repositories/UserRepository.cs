namespace Infrastructure.Repositories;

using Domain.Entities;
using Domain.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

public class UserRepository(ChatDatabaseContext databaseContext) : IUserRepository {

  public async Task<User?> GetByIdAsync(int id) {
    return await databaseContext.Users.FindAsync(id);
  }
  
  public async Task<User?> GetByNameAsync(string name) {
    return await databaseContext.Users.Where(u => u.Name == name).FirstOrDefaultAsync();
  }
  
  public async Task<PagedResult<User>> GetPagedAsync(int pageSize,
    int? lastId = null) {

    IQueryable<User> query = databaseContext.Users.OrderByDescending(u => u.Id).AsNoTracking();

    if (lastId.HasValue) {
      query = query.Where(u => u.Id < lastId.Value);
    }
    
    IEnumerable<User> users = await query.Take(pageSize).ToListAsync();
    
    return new PagedResult<User>(users);
  }

  public async Task AddAsync(User user) {
    await databaseContext.Users.AddAsync(user);
  }
}
