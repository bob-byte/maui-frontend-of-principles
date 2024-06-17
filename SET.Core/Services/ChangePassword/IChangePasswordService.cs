using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services.ChangePassword;
public interface IChangePasswordService
{
    Task ChangePasswordAsync( string email, string newPassword );
    Task SendEmailAsync( string emailAddress );
    Task<string> GetConfirmationCodeAsync();
}
