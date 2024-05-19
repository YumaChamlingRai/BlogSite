using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Bislerium.DTOs;
using Bislerium.Models;
using Bislerium.Models.Request;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Bislerium.Pages;

public partial class Posts
{
    private string Type = "";
    private string Message = "";
    private Guid InputFileId = Guid.NewGuid();
    private List<string> ImageURL = [];

    [CascadingParameter] private GlobalState? GlobalState { get; set; }

    private BlogRequestDto _blogRequestDto = new();

    private async Task OnInputFileChanged(InputFileChangeEventArgs e)
    {
        using var formData = new MultipartFormDataContent();

        var files = e.GetMultipleFiles(5);

        if (files.Any(x => x.Size > 3 * 1024 * 1024))
        {
            Type = "warning";
            Message = "Please upload images valid up to 3MB";
            InputFileId = Guid.NewGuid();
            return;
        }

        foreach (var file in e.GetMultipleFiles(5))
        {
            var fileContent = new StreamContent(file.OpenReadStream(long.MaxValue));

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            formData.Add(content: fileContent, name: "Files", fileName: file.Name);
        }

        var response = await HttpClient.PostAsync("https://localhost:44340/api/ImageUpload/UploadImage", formData);

        var uploadedResult = await response.Content.ReadFromJsonAsync<List<string>>();

        ImageURL = uploadedResult!;
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            var blog = new BlogRequestDto()
            {
                Title = _blogRequestDto.Title,
                Body = _blogRequestDto.Body,
                Location = _blogRequestDto.Location,
                Reaction = _blogRequestDto.Reaction,
                Images = ImageURL
            };

            var jsonRequest = JsonSerializer.Serialize(blog);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await ApiService.PostAsync("Post/CreatePost", content);

            _blogRequestDto = new();

            Type = "success";

            Message = "Post Successfully Created";
        }
        catch (Exception)
        {
            Type = "danger";
            Message = "An exception occured while uploading your post request.";
        }
    }

    private void HandleReactionType(ChangeEventArgs e)
    {
        var reaction = e.Value?.ToString();

        if (reaction != null)
        {
            _blogRequestDto.Reaction = reaction;
        }
    }
}