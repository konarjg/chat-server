namespace ChatServer.Services;

using Chat;
using Domain.Entities;
using Domain.Interfaces;
using Grpc.Core;
using Mappers;
using DomainChat = Domain.Entities.Chat;
using DomainMessage = Domain.Entities.Message;
using Chat = Chat.Chat;

public class ChatServiceImpl(IChatService chatService, IMessageService messageService, IMessageStream messageStream) : ChatService.ChatServiceBase {

  public override async Task<Chat> CreateChat(CreateChatRequest request,
    ServerCallContext context) {

    int userId = 0;

    DomainChat domainChat = await chatService.CreateChatAsync(ChatMapper.ToEntity(request,userId));
    return ChatMapper.ToDto(domainChat);
  }

  public override async Task<GetChatsResponse> GetChats(GetChatsRequest request,
    ServerCallContext context) {
    
    int userId = 0;

    PagedResult<DomainChat> domainChats = await chatService.GetChatsAsync(ChatMapper.ToFilters(request,userId));
    return ChatMapper.ToResponse(domainChats);
  }

  public override async Task<GetMessageHistoryResponse> GetMessageHistory(GetMessageHistoryRequest request,
    ServerCallContext context) {
    
    PagedResult<DomainMessage> domainMessages = await messageService.GetMessageHistoryAsync(MessageMapper.ToFilters(request));
    return MessageMapper.ToResponse(domainMessages);
  }

  public override Task ChatStream(IAsyncStreamReader<ClientToServerMessage> requestStream,
    IServerStreamWriter<ServerToClientMessage> responseStream,
    ServerCallContext context) {

    int userId = 0;
    
    MessageReceivedCallback outgoingCallback = async (domainMessage) =>
    {
      var serverMessage = MessageMapper.ToServerToClientMessage(domainMessage);
      await responseStream.WriteAsync(serverMessage);
    };
  }
}
