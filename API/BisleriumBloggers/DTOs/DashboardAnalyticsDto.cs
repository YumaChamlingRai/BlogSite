namespace BisleriumBloggers.DTOs;

public class DashboardAnalyticsDto
{
    public List<DashboardDetails> ReactionCount { get; set; }
    
    public List<DashboardDetails> PopularBloggersCount { get; set; }
    
    public List<DashboardDetails> PopularBlogsCount { get; set; }
}

public class DashboardDetails
{
    public string Title { get; set; }
    
    public int Count { get; set; }
}