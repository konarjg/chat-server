namespace Infrastructure.Repositories;

using Domain.Entities;
using Domain.Ports.Repositories;
using Microsoft.EntityFrameworkCore;

public class ChatRepository(ChatDatabaseContext databaseContext) : IChatRepository {

  public async Task<Chat?> GetByIdAsync(int id) {
    return await databaseContext.Chats.FindAsync(id);
  }
  public async Task<Chat?> GetByUserIdsAsync(int senderId,
    int receiverId) {

    return await databaseContext.Chats.Where(c => (c.SenderId == senderId || c.ReceiverId == senderId)
                                                                         && (c.ReceiverId == receiverId || c.SenderId == receiverId))
                                .FirstOrDefaultAsync();
  }
  
  public async Task<PagedResult<Chat>> GetPagedAsync(int userId,
    int pageSize,
    int? lastId) {
    
    IQueryable<Chat> query = databaseContext.Chats.Where(c => c.SenderId == userId || c.ReceiverId == userId)
                                               .OrderByDescending(c => c.Id)
                                               .AsNoTracking();

    if (lastId.HasValue) {
      query = query.Where(m => m.Id < lastId.Value);
    }
    
    IEnumerable<Chat> chats = await query.Take(pageSize).ToListAsync();

    return new PagedResult<Chat>(chats);
  }
  
  public async Task AddAsync(Chat chat) {
    await databaseContext.Chats.AddAsync(chat);
  }
}
