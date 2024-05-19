using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }

        public string UserName { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string? MobileNo { get; set; }

        public string EmailAddress { get; set; } = null!;
        
        public string? ImageURL { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        public virtual ICollection<Blog>? Blogs { get; set; }

        public virtual ICollection<BlogImage>? BlogImages { get; set; }

        public virtual ICollection<BlogLog>? BlogLogs { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<CommentLog>? CommentLogs { get; set; }

        public virtual ICollection<Notification>? Notifications { get; set; }

        public virtual ICollection<Reaction>? Reactions { get; set; }
    }
}

