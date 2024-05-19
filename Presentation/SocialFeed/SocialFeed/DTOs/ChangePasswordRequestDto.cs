namespace Bislerium.DTOs;

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; }

    public string NewPassword { get; set; }

    public string ConfirmPassword { get; set; }
}