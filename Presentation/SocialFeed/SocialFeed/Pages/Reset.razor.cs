namespace Bislerium.Pages;

public partial class Reset
{
    private string EmailAddress = "";
    private string Type = "";
    private string Message = "";

    private async Task HandleValidSubmit()
    {
        try
        {
            await APIService.GetAsync<object>($"UserProfile/ResetPassword?emailAddress={EmailAddress}");

            NavManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Type = "danger";
            Message = "The username or password is incorrect. Try again.";
        }
    }
}