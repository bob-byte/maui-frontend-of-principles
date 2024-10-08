
namespace Principles.Core.Models;

public class LaunchExternalOAuthResult
{
    public LaunchExternalOAuthResult( LaunchType type, string? code, string? error )
    {
        Type = type;
        Code = code;
        Error = error;
    }

    public LaunchType Type { get; }
    public string? Code { get; }
    public string? Error { get; }
}
