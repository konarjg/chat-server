namespace Domain.Interfaces;

using Entities;

public interface IChatService {
  Task<PagedResult<Chat>> GetChatsAsync(ChatFilters filters);
  Task<Chat> CreateChatAsync(CreateChatCommand command);
}
