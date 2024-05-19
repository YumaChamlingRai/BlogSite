namespace BisleriumBloggers.DTOs;

public class UpVoteDownVoteActionDto
{
    public int? BlogId { get; set; }

    public int? CommentId { get; set; }

    public int? ReactionId { get; set; }

    public string? Comment { get; set; }
}