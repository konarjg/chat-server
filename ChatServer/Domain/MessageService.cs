namespace Domain;

using Entities;
using Exceptions;
using Interfaces;
using Ports.Repositories;

public class MessageService(IChatRepository chatRepository, IMessageRepository messageRepository, IUnitOfWork unitOfWork) : IMessageService {

  public async Task<PagedResult<Message>> GetMessageHistoryAsync(MessageHistoryFilters filters) {
    return await messageRepository.GetMessageHistoryAsync(filters.ChatId, filters.PageSize, filters.LastId);
  }

  public async Task<Message> CreateMessageAsync(CreateMessageCommand command) {
    Chat? chat = await chatRepository.GetByIdAsync(command.ChatId);

    if (chat == null) {
      throw new ChatNotFoundException($"Chat with id {command.ChatId} does not exist.");
    }

    if (chat.SenderId != command.SenderId && chat.ReceiverId != command.SenderId) {
      throw new UserNotInChatException($"User with id {command.SenderId} is not a member of the chat with id {command.ChatId}.");
    }

    Message message = new() {
      ChatId = command.ChatId,
      SenderId = command.SenderId,
      AesEncryptedContent = command.AesEncryptedContent
    };

    await messageRepository.AddAsync(message);
    await unitOfWork.CompleteAsync();

    return message;
  }
}
