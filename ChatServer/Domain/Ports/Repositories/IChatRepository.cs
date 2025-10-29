namespace Domain.Ports.Repositories;

using Entities;

public interface IChatRepository {
  Task<Chat?> GetByIdAsync(int id);
  Task<Chat?> GetByUserIdsAsync(int senderId, int receiverId);
  Task<PagedResult<Chat>> GetPagedAsync(int userId, int pageSize, int? lastId);
  Task AddAsync(Chat chat);
}
