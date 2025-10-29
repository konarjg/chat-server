namespace ChatServer.Mappers;

using Chat;
using Domain.Entities;
using Google.Protobuf;
using Message = Domain.Entities.Message;
using MessageDto = Chat.Message;

public static class MessageMapper {
  public static MessageDto ToDto(Message message) {
    return new MessageDto() {
      Id = message.Id,
      ChatId = message.ChatId,
      SenderId = message.SenderId,
      AesEncryptedContent = ByteString.CopyFrom(message.AesEncryptedContent)
    };
  }

  public static MessageHistoryFilters ToFilters(GetMessageHistoryRequest request) {
    return new MessageHistoryFilters(request.ChatId,request.PageSize,request.LastId);
  }

  public static GetMessageHistoryResponse ToResponse(PagedResult<Message> messages) {
    GetMessageHistoryResponse response = new();
    response.Messages.AddRange(messages.Items.Select(ToDto));
    
    return response;
  }
}
