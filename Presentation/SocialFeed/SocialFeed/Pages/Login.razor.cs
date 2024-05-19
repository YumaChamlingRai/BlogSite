using System.Text.Json;
using Bislerium.DTOs;
using Bislerium.Models;
using Bislerium.Models.Request;
using Bislerium.Models.Response;
using Microsoft.AspNetCore.Components;

namespace Bislerium.Pages;

public partial class Login
{
    private string Type = "";
    private string Message = "";

    [CascadingParameter] private GlobalState? GlobalState { get; set; }

    private readonly LoginRequestDto _loginRequestDto = new();

    private async Task HandleValidSubmit()
    {
        var loginRequest = new LoginRequestDto()
        {
            EmailAddress = _loginRequestDto.EmailAddress,
            Password = _loginRequestDto.Password
        };

        var jsonRequest = JsonSerializer.Serialize(loginRequest);

        var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var authResult = await ApiService.PostAsync<LoginResponseDto>("Authentication/Login", content);

            await LocalStorage.SetItemAsync("access_token", authResult.Token ?? "");

            if (GlobalState == null)
            {
                var globalState = new GlobalState()
                {
                    UserId = authResult.Id,
                    RoleId = authResult.RoleId,
                    Name = authResult.Name,
                    RoleName = authResult.Role,
                    ImageUrl = authResult.ImageUrl,
                };

                GlobalState = globalState;
            }

            var navigation = GlobalState.RoleName == "Admin" ? "/dashboard" : "/feeds";

            NavigationManager.NavigateTo(navigation);
        }
        catch (Exception ex)
        {
            Type = "warning";
            Message = $"The username or password is incorrect.";
        }
    }
}