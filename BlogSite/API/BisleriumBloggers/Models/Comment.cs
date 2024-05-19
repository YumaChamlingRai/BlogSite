using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class Comment : Base.Base
    {
        public string Text { get; set; }

        public bool IsCommentForBlog { get; set; }

        public bool IsCommentForComment { get; set; }

        public int? BlogId { get; set; }

        public int? CommentId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog? Blog { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comments { get; set; }
    }
}
