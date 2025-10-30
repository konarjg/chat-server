namespace ChatServer.Mappers;

using Chat;
using Domain.Entities;
using User = Domain.Entities.User;
using UserDto = Chat.User;

public class UserMapper {
  public static UserFilters ToFilters(GetUsersRequest request) {
    return new UserFilters(request.PageSize,request.HasLastId ? request.LastId : null);
  }
  
  public static GetUsersResponse ToResponse(PagedResult<User> users) {
    GetUsersResponse response = new();
    response.Users.AddRange(users.Items.Select(ToDto));

    return response;
  }

  public static UserDto ToDto(User user) {
    return new UserDto() {
      Id = user.Id,
      Name = user.Name,
      PublicKey = user.PublicKey
    };
  }
}
