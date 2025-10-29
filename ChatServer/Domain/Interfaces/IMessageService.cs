namespace Domain.Interfaces;

using Entities;

public interface IMessageService {
  Task<PagedResult<Message>> GetMessageHistoryAsync(MessageHistoryFilters filters);
  Task<Message> CreateMessageAsync(CreateMessageCommand command);
}
