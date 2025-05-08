using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services.Managers;
public class UserManagerService : IUserManagerService
{
    private readonly IDatabaseService _db;
    public UserManagerService( IDatabaseService db )
    {
        _db = db;
    }
    public async Task SaveUserFromDtoAsync( UserDto userDto )
    {
        var user = new UserInfo
        {
            Name = userDto.Name,
            Email = userDto.Email,
            Mission = userDto.Mission ?? string.Empty,
            MainSlogan = userDto.MainSlogan ?? string.Empty,
            Gender = userDto.Gender,
            IsSynced = true
        };

        var existingUsers = await _db.GetAllAsync<UserInfo>();
        foreach (var u in existingUsers)
            await _db.DeleteAsync( u );

        await _db.InsertAsync( user );
    }
}
