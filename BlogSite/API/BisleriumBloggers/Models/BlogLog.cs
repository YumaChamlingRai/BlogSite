using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class BlogLog : Base.Base
    {
        public int BlogId { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string Location { get; set; }

        public string Reaction { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog? Blog { get; set; }
    }
}

