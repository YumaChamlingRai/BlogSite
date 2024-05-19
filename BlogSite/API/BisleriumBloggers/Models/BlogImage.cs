using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class BlogImage : Base.Base
    {
        public string ImageURL { get; set; }

        public int BlogId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }
    }
}

