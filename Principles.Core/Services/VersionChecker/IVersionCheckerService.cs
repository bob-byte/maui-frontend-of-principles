using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services;
public interface IVersionCheckerService
{
    Task<AppVersionInfo> GetAppVersionAsync( string language );
}
