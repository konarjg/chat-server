namespace ChatServer.Mappers;

using Chat;
using Domain.Entities;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Chat = Domain.Entities.Chat;
using ChatDto = Chat.Chat;

public static class ChatMapper {
  public static ChatDto ToDto(Chat chat) {
    return new ChatDto {
      Id = chat.Id,
      SenderId = chat.SenderId,
      ReceiverId = chat.ReceiverId,
      SenderEncryptedAesKey = ByteString.CopyFrom(chat.SenderEncryptedAesKey),
      ReceiverEncryptedAesKey = ByteString.CopyFrom(chat.ReceiverEncryptedAesKey)
    };
  }

  public static CreateChatCommand ToEntity(CreateChatRequest request, int senderId) {
    return new CreateChatCommand(senderId,request.ReceiverId,request.SenderEncryptedAesKey.ToByteArray(),request.ReceiverEncryptedAesKey.ToByteArray());
  }

  public static ChatFilters ToFilters(GetChatsRequest request,
    int senderId) {
    
    return new ChatFilters(senderId,request.PageSize,request.LastId);
  }

  public static GetChatsResponse ToResponse(PagedResult<Chat> chats) {

    GetChatsResponse response = new();
    response.Chats.AddRange(chats.Items.Select(ToDto));
    
    return response;
  }
}
