using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services;

public interface IChangePasswordService
{
    Task ChangePasswordAsync( string email, string newPassword );
    Task<int> GeneratedCodeAsync( string emailWhereSendCode );
}
