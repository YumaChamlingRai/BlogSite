using System.Text.Json;
using Bislerium.DTOs;

namespace Bislerium.Pages;

public partial class Password
{
    private string Type = "";
    private string Message = "";
    private ChangePasswordRequestDto _changePassword = new();

    private async Task HandleValidSubmit()
    {
        try
        {
            if (_changePassword.ConfirmPassword != _changePassword.NewPassword)
            {
                Type = "danger";
                Message = "Please enter the same values for your new and confirmed password and try again.";

                return;
            }

            var changePassword = new ChangePasswordRequestDto()
            {
                CurrentPassword = _changePassword.CurrentPassword,
                NewPassword = _changePassword.NewPassword,
                ConfirmPassword = _changePassword.ConfirmPassword,
            };

            var jsonRequest = JsonSerializer.Serialize(changePassword);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await APIService.PostAsync("UserProfile/ChangePassword", content);

            _changePassword = new();

            Type = "success";
            Message = "Password Successfully Updated";
        }
        catch (Exception ex)
        {
            Type = "danger";
            Message = "Your password does not match, please try with a valid password.";
        }
    }
}