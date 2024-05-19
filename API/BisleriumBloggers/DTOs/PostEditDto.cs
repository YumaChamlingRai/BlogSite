namespace BisleriumBloggers.DTOs;

public class PostEditDto
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    public string Location { get; set; }

    public string Reaction { get; set; }

    public List<string>? Images { get; set; }
}
