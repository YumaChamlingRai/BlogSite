namespace BisleriumBloggers.DTOs;

public class NotificationDetailsDto
{
    public string TimePeriod { get; set; }
    
    public List<Notifications> Notifications { get; set; }
}

public class Notifications
{
    public int Id { get; set; }
    
    public string SentByUser { get; set; }
    
    public string SentImageUrl { get; set; }
    
    public  string Title { get; set; }
    
    public string Content { get; set; }
    
    public bool IsSeen { get; set; }
}