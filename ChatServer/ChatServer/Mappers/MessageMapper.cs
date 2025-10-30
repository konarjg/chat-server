namespace ChatServer.Mappers;

using System.Runtime.CompilerServices;
using Chat;
using Domain.Entities;
using Google.Protobuf;
using Grpc.Core;
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
    return new MessageHistoryFilters(request.ChatId,request.PageSize,request.HasLastId ? request.LastId : null);
  }

  public static GetMessageHistoryResponse ToResponse(PagedResult<Message> messages) {
    GetMessageHistoryResponse response = new();
    response.Messages.AddRange(messages.Items.Select(ToDto));
    
    return response;
  }

  public static MessageStreamEntry ToStreamEntry(SendMessageRequest request) {
    return new MessageStreamEntry(request.ChatId,request.AesEncryptedContent.ToByteArray());
  }
}
