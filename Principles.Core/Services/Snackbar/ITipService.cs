using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services;
public interface ITipService
{
    Task ShowAsync( string message, string actionText = null, Action? action = null );
}
