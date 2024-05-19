using System.Text.Json;
using Bislerium.Models.Request;

namespace Bislerium.Pages;

public partial class Profile
{
    private string Type = "";
    private string Message = "";
    private ProfileRequestDto _profileRequestDto = new();

    protected override async Task OnInitializedAsync()
    {
        var isAccessTokenValid = await ApiService.IsUserLoggedIn();

        if (isAccessTokenValid)
        {
            var userDetails = await ApiService.GetAsync<ProfileRequestDto>("UserProfile/GetUserDetails");

            _profileRequestDto = userDetails!;
        }
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            var profileDetails = new ProfileRequestDto()
            {
                FullName = _profileRequestDto.FullName,
                MobileNumber = _profileRequestDto.MobileNumber,
                EmailAddress = _profileRequestDto.EmailAddress,
                RoleId = _profileRequestDto.RoleId,
                RoleName = _profileRequestDto.RoleName,
                UserId = _profileRequestDto.UserId,
                Username = _profileRequestDto.Username
            };

            var jsonRequest = JsonSerializer.Serialize(profileDetails);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await ApiService.UpdateAsync("UserProfile/UpdateProfileDetails", content);

            Type = "success";
            Message = "Profile Details Successfully Updated";
        }
        catch (Exception ex)
        {
            Type = "danger";
            Message = "Your profile could not be updated, please try again.";
        }
    }
}