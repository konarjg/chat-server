namespace ChatServer.Services;

using System.Runtime.CompilerServices;
using System.Security.Claims;
using Chat;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Grpc.Core;
using Mappers;
using Microsoft.Extensions.Logging;
using DomainChat = Domain.Entities.Chat;
using DomainMessage = Domain.Entities.Message;
using Chat = Chat.Chat;

public class ChatServiceImpl(
  IChatService chatService,
  IMessageService messageService,
  IMessageStream messageStream,
  ILogger<ChatServiceImpl> logger) : ChatService.ChatServiceBase {

  public override async Task<Chat> CreateChat(CreateChatRequest request, ServerCallContext context) {
    try {
      int userId = GetUserIdFromContext(context);
      DomainChat domainChat = await chatService.CreateChatAsync(ChatMapper.ToEntity(request, userId));
      return ChatMapper.ToDto(domainChat);
    }
    catch (SelfChatException ex) {
      throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
    }
    catch (UserNotFoundException ex) {
      throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
    }
    catch (ChatAlreadyExistsException ex) {
      throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
    }
  }

  public override async Task<GetChatsResponse> GetChats(GetChatsRequest request, ServerCallContext context) {
    int userId = GetUserIdFromContext(context);
    PagedResult<DomainChat> domainChats = await chatService.GetChatsAsync(ChatMapper.ToFilters(request, userId));
    return ChatMapper.ToResponse(domainChats);
  }

  public override async Task<GetMessageHistoryResponse> GetMessageHistory(GetMessageHistoryRequest request, ServerCallContext context) {
    PagedResult<DomainMessage> domainMessages = await messageService.GetMessageHistoryAsync(MessageMapper.ToFilters(request));
    return MessageMapper.ToResponse(domainMessages);
  }

  public override async Task ChatStream(IAsyncStreamReader<ClientToServerMessage> requestStream, IServerStreamWriter<ServerToClientMessage> responseStream, ServerCallContext context) {
    int userId = GetUserIdFromContext(context);

    try {
      MessageReceivedCallback outgoingCallback = async (domainMessage) => {
        ServerToClientMessage serverMessage = new() {
          NewMessage = MessageMapper.ToDto(domainMessage)
        };
        await responseStream.WriteAsync(serverMessage);
      };

      IAsyncEnumerable<MessageStreamEntry> incomingMessages = MapRequestStream(requestStream, context.CancellationToken);
      await messageStream.ProcessMessageStreamAsync(userId, incomingMessages, outgoingCallback, context.CancellationToken);
    }
    catch (ChatNotFoundException ex) {
      logger.LogWarning(ex, "User {UserId} tried to send a message to a non-existent chat.", userId);
      throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
    }
    catch (UserNotInChatException ex) {
      logger.LogWarning(ex, "User {UserId} tried to send a message to a chat they are not in.", userId);
      throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
    }
    catch (OperationCanceledException) {
      logger.LogInformation("ChatStream was cancelled by the client for user {UserId}.", userId);
    }
  }

  private async IAsyncEnumerable<MessageStreamEntry> MapRequestStream(
    IAsyncStreamReader<ClientToServerMessage> requestStream,
    [EnumeratorCancellation] CancellationToken cancellationToken) {
    await foreach (var request in requestStream.ReadAllAsync(cancellationToken)) {
      if (request.PayloadCase == ClientToServerMessage.PayloadOneofCase.SendMessage) {
        yield return MessageMapper.ToStreamEntry(request.SendMessage);
      }
    }
  }
  
  private int GetUserIdFromContext(ServerCallContext context) {
    var httpContext = context.GetHttpContext();
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

    if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId)) {
      throw new RpcException(new Status(StatusCode.Unauthenticated, "User identity could not be determined."));
    }

    return userId;
  }
}