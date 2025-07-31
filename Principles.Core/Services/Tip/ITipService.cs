using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services;
public interface ITipService
{
    Task ShowSnackbarAsync( string message );
    Task ShowSnackbarAsync( string message, TimeSpan duration );
    Task ShowToastAsync( string message );
    Task ShowToastAsync( string message, TipDuration duration );
}
