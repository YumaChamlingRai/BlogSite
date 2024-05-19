using Bislerium.DTOs;
using Bislerium.Models;
using Bislerium.Models.Request;
using Microsoft.AspNetCore.Components;

namespace Bislerium.Pages;

public partial class Home
{
    [CascadingParameter] private GlobalState _globalState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var isAccessTokenValid = await Service.IsUserLoggedIn();

        if (isAccessTokenValid)
        {
            var userDetails = await Service.GetAsync<ProfileRequestDto>("UserProfile/GetUserDetails");

            var user = userDetails!;

            _globalState = new GlobalState()
            {
                Name = user.FullName,
                ImageUrl = user.ImageURL,
                RoleName = user.RoleName,
                UserId = user.UserId,
                RoleId = user.RoleId
            };

            NavManager.NavigateTo(_globalState.RoleName == "Admin" ? "/dashboard" : "/feeds");
        }
        else
        {
            NavManager.NavigateTo("/feeds");
        }
    }
}