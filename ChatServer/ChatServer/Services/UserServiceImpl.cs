namespace ChatServer.Services;

using Chat;
using Domain.Entities;
using Domain.Interfaces;
using Grpc.Core;
using Mappers;
using Microsoft.AspNetCore.Identity;
using DomainUser = Domain.Entities.User;

public class UserServiceImpl(IUserService userService) : UserService.UserServiceBase{
  public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request,
    ServerCallContext context) {

    UserFilters filters = UserMapper.ToFilters(request);
    PagedResult<DomainUser> users = await userService.GetUsersAsync(filters);
    
    return UserMapper.ToResponse(users);
  }
}
