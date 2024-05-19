using Bislerium.Models.Response;
using Microsoft.AspNetCore.Components;

namespace Bislerium.Pages;

public partial class Feeds
{
    private List<FeedResponseDto> Blogs = new();

    protected override async Task OnInitializedAsync()
    {
        var blogDetails = await ApiService.GetAsync<List<FeedResponseDto>>("Feed/GetHomePagePosts");

        Blogs = blogDetails!;
    }

    private void NavigateToBlogDetails(int blogId)
    {
        NavManager.NavigateTo($"/blogs/{blogId}");
    }

    private async Task HandleFeedCategory(ChangeEventArgs e)
    {
        var sortBy = e.Value?.ToString();

        var blogDetails = await ApiService.GetAsync<List<FeedResponseDto>>($"Feed/GetHomePagePosts?sortBy={sortBy}");

        Blogs = blogDetails!;
    }
}