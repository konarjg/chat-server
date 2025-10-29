namespace Infrastructure.Repositories;

using Domain.Entities;
using Domain.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

public class MessageRepository(ChatDatabaseContext databaseContext) : IMessageRepository {

  public async Task<PagedResult<Message>> GetMessageHistoryAsync(int chatId,
    int pageSize,
    int? lastId = null) {

    IQueryable<Message> query = databaseContext.Messages.Where(m => m.ChatId == chatId)
                                               .OrderByDescending(m => m.Id)
                                               .AsNoTracking();

    if (lastId.HasValue) {
      query = query.Where(m => m.Id < lastId.Value);
    }
    
    IEnumerable<Message> messages = await query.Take(pageSize).ToListAsync();

    return new PagedResult<Message>(messages);
  }
  
  public async Task AddAsync(Message message) {
    await databaseContext.Messages.AddAsync(message);
  }
}
