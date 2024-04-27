using System;
namespace SET.MAUI.Models
{
    public class LoggedOutEventArgs : EventArgs
    {
        public string UserId { get; init; }
        public string Token { get; init; }
    }
}

