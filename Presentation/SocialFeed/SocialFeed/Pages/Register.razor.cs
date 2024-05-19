using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Bislerium.Models.Request;
using Microsoft.AspNetCore.Components.Forms;

namespace Bislerium.Pages;

public partial class Register
{
    private string Type = "";
    private string Message = "";
    private string ImageURL = "";
    private readonly RegistrationRequestDto _registrationRequestDto = new();

    private async Task OnInputFileChanged(InputFileChangeEventArgs e)
    {
        using var formData = new MultipartFormDataContent();

        foreach (var file in e.GetMultipleFiles(1))
        {
            var fileContent = new StreamContent(file.OpenReadStream(long.MaxValue));

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            formData.Add(content: fileContent, name: "Files", fileName: file.Name);
        }

        var response = await HttpClient.PostAsync("https://localhost:44340/api/ImageUpload/UploadImage", formData);

        var uploadedResult = await response.Content.ReadFromJsonAsync<List<string>>();

        ImageURL = uploadedResult!.FirstOrDefault()!;
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            var userRegistration = new RegistrationRequestDto()
            {
                EmailAddress = _registrationRequestDto.EmailAddress,
                Password = _registrationRequestDto.Password,
                Username = _registrationRequestDto.Username,
                FullName = _registrationRequestDto.FullName,
                MobileNumber = _registrationRequestDto.MobileNumber,
                ImageURL = string.IsNullOrEmpty(ImageURL) ? null : ImageURL
            };

            var jsonRequest = JsonSerializer.Serialize(userRegistration);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await ApiService.PostAsync("Authentication/Register", content);

            const string navigation = "/login";

            NavigationManager.NavigateTo(navigation);
        }
        catch (Exception ex)
        {
            Type = "warning";
            Message = "The username with the following username or email already exists, please try again.";
        }
    }
}