using System.Text.Json;
using Bislerium.DTOs;
using Microsoft.AspNetCore.Components;

namespace Bislerium.Pages;

public partial class BlogDetails
{
    [Parameter] 
    public int blogId { get; set; }

    private BlogResponseDto _blogDetails = new();
    private List<BlogResponseDto> _blogLogsDetails = new();
    
    private string Type = "";
    private string Message = "";

    protected override async Task OnInitializedAsync()
    {
        if (blogId == 0) return;

        var blogDetails = await APIService.GetAsync<BlogResponseDto>($"Post/GetPostById?postId={blogId}");

        _blogDetails = blogDetails!;

        var blogLogsDetails = await APIService.GetAsync<List<BlogResponseDto>>($"Post/GetPostLogs?postId={blogId}");

        _blogLogsDetails = blogLogsDetails!;
    }

    private async Task HandleValidSubmit()
    {
        try
        {
            var jsonRequest = JsonSerializer.Serialize(_blogDetails);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await APIService.UpdateAsync("Post/UpdatePost", content);

            Type = "success";
            Message = "Blog details successfully updated.";

            await OnInitializedAsync();
        }
        catch (Exception ex)
        {
            Type = "danger";
            Message = "An exception occured while updating your blog, please try again.";
        }
    }

    private async Task DeleteBlog()
    {
        try
        {
            await APIService.DeleteAsync($"Post/DeletePost?postId={_blogDetails.Id}");

            NavManager.NavigateTo("/my-blogs");
        }
        catch (Exception ex)
        {
            Type = "danger";
            Message = "An exception occured while updating your blog, please try again.";
        }
    }

    private void HandleReactionType(ChangeEventArgs e)
    {
        var reaction = e.Value?.ToString();

        if (reaction != null)
        {
            _blogDetails.Reaction = reaction;
        }
    }
}