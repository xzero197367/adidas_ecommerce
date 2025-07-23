


using Models.People;

namespace Adidas.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = new Guid();

        // foreign keys
        public string? AddedById { get; set; }

        // const properties
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // navigation properties
        public virtual User? AddedBy { get; set; }
    }
}
