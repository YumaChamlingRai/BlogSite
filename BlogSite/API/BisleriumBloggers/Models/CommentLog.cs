using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class CommentLog : Base.Base
    {
        public int CommentId { get; set; }

        public string Text { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment? Comment { get; set; }
    }
}

