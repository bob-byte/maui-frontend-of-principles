namespace Principles.Core.Models;

public class ChangePasswordRequest
{
    public string Email { get; set; }
    public string NewPassword { get; set; }
    public int Code { get; set; }
}
