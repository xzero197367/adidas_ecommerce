using Models.People;

namespace Adidas.Models;

public class BaseAuditableEntity: BaseEntity
{
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