using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class Reaction : Base.Base
    {
        public int ReactionId { get; set; }

        public bool IsReactedForBlog { get; set; }

        public bool IsReactedForComment { get; set; }

        public int? BlogId { get; set; }

        public int? CommentId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog? Blog { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }
    }

}

