using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BisleriumBloggers.Models.Base
{
    public class Base
    {
        [Key]
        public int Id { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public int CreatedBy { get; set; } = new();

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public int? LastModifiedBy { get; set; }

        public DateTime? LastModifiedOn { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DeletedOn { get; set; }
        
        [ForeignKey("CreatedBy")]
        public virtual User? CreatedUser { get; set; }
        
        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedUser { get; set; }
        
        [ForeignKey("DeletedBy")]
        public virtual User? DeletedUser { get; set; }
    }
}

