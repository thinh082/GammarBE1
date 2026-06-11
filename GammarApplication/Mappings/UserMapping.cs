using GammarApplication.DTOs.Users;
using GammarDomain.Entities;

namespace GammarApplication.Mappings;

public static class UserMapping
{
    public static UserDto ToDto(this User user) => new(user.Id, user.Email, null);
}
