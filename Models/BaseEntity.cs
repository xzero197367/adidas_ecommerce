


using System.ComponentModel.DataAnnotations;
using Models.People;

namespace Adidas.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // foreign keys
        public string? AddedById { get; set; }

        // const properties
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // navigation properties
        public virtual User? AddedBy { get; set; }
    }
}
