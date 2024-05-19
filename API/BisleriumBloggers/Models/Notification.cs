using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models
{
    public class Notification : Base.Base
    {
        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public bool IsSeen { get; set; }

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public virtual User? Receiver { get; set; }
    }
}

