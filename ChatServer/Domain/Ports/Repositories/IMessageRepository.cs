namespace Domain.Ports.Repositories;

using Entities;

public interface IMessageRepository {
  Task<PagedResult<Message>> GetMessageHistoryAsync(int chatId,
    int pageSize,
    int? lastId = null);
  Task AddAsync(Message message);
}
