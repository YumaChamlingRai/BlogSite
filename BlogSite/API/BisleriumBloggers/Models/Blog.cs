namespace BisleriumBloggers.Models
{
    public class Blog : Base.Base
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public string Location { get; set; }

        public string Reaction { get; set; }

        public virtual ICollection<BlogImage>? BlogImages { get; set; }
    }
}

