namespace Bislerium.DTOs
{
    public class DashboardResponseDto
    {
        public List<DashboardCount> ReactionCount { get; set; }

        public List<DashboardCount> PopularBloggersCount { get; set; }

        public List<DashboardCount> PopularBlogsCount { get; set; }
    }

    public class DashboardCount
    {
        public string Title { get; set; }

        public int Count { get; set; }
    }
}
