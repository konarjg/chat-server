namespace Domain;

using Entities;
using Interfaces;
using Ports.Repositories;

public class UserService(IUserRepository userRepository) : IUserService {

  public async Task<PagedResult<User>> GetUsersAsync(UserFilters filters) {
    return await userRepository.GetPagedAsync(filters.PageSize,filters.LastId);
  }
}
