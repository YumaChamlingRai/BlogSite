using System.Text.Json;
using Bislerium.Models.Request;
using Bislerium.Models.Response;
using Microsoft.AspNetCore.Components;

namespace Bislerium.Pages;

public partial class Blogs
{
    [Parameter] public int blogId { get; set; }

    public int action { get; set; }
    private readonly ReactionRequestDto _reaction = new();
    private string Type = "";
    private string Message = "";
    private bool IsCommentModalOpenForBlog = false;
    private FeedResponseDto? BlogDetails = new();
    private bool IsLoggedIn { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoggedIn = await APIService.IsUserLoggedIn();

        if (blogId == 0) return;

        var blogDetails = await APIService.GetAsync<FeedResponseDto>($"Reaction/GetBlogDetails?blogId={blogId}");

        BlogDetails = blogDetails!;
    }

    private void OpenCommentModalForBlog()
    {
        IsCommentModalOpenForBlog = true;
    }

    private async Task UpVoteDownVoteBlog(int reactionId)
    {
        try
        {
            var reactionModel = new ReactionRequestDto()
            {
                BlogId = blogId,
                ReactionId = reactionId
            };

            var jsonRequest = JsonSerializer.Serialize(reactionModel);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await APIService.PostAsync("Reaction/UpVoteDownVoteBlog", content);

            await OnInitializedAsync();

            Type = "success";

            Message = reactionId == 1 ? "Blog successfully up voted" : "Blog successfully down voted";
        }
        catch (Exception e)
        {
            Type = "danger";
            Message = "An exception occured while reacting the blog, try again";
            Console.WriteLine(e);
        }
    }

    private async Task CommentForBlog()
    {
        try
        {
            var reactionModel = new ReactionRequestDto()
            {
                BlogId = blogId,
                Comment = _reaction.Comment
            };

            var jsonRequest = JsonSerializer.Serialize(reactionModel);

            var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            await APIService.PostAsync("Reaction/CommentForBlog", content);

            action = 1;

            await OnInitializedAsync();

            _reaction.Comment = "";

            Type = "success";
            IsCommentModalOpenForBlog = false;
            Message = "Comment successfully uploaded";
        }
        catch (Exception e)
        {
            Type = "danger";
            Message = "An exception occured while uploading your comment, try again";
            Console.WriteLine(e);
        }
    }
}