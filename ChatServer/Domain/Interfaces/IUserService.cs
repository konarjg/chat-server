namespace Domain.Interfaces;

using Entities;

public interface IUserService {
  Task<PagedResult<User>> GetUsersAsync(UserFilters filters);
}
