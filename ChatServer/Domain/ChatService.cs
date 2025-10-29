namespace Domain;

using Entities;
using Exceptions;
using Interfaces;
using Ports.Repositories;

public class ChatService(IChatRepository chatRepository, IUserRepository userRepository, IUnitOfWork unitOfWork) : IChatService{

  public async Task<PagedResult<Chat>> GetChatsAsync(ChatFilters filters) {
    return await chatRepository.GetPagedAsync(filters.UserId, filters.PageSize, filters.LastId);
  }

  public async Task<Chat> CreateChatAsync(CreateChatCommand command) {
    
    if (command.SenderId == command.ReceiverId) {
      throw new SelfChatException($"Sender id and receiver id cannot be identical.");
    }
    
    if (await userRepository.GetByIdAsync(command.SenderId) == null) {
      throw new UserNotFoundException($"User with provider sender id: {command.SenderId} does not exist.");
    }
    
    if (await userRepository.GetByIdAsync(command.ReceiverId) == null) {
      throw new UserNotFoundException($"User with provider receiver id: {command.ReceiverId} does not exist.");
    }

    if (await chatRepository.GetByUserIdsAsync(command.SenderId,command.ReceiverId) != null) {
      throw new ChatAlreadyExistsException($"Chat between user with id {command.SenderId} and user with id {command.ReceiverId} already exists.");
    }

    Chat chat = new() {
      SenderId = command.SenderId,
      ReceiverId = command.ReceiverId,
      ReceiverEncryptedAesKey = command.ReceiverEncryptedAesKey,
      SenderEncryptedAesKey = command.SenderEncryptedAesKey
    };

    await chatRepository.AddAsync(chat);
    await unitOfWork.CompleteAsync();

    return chat;
  }
}
