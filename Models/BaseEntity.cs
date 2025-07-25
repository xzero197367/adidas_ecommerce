
using System.ComponentModel.DataAnnotations;


namespace Adidas.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
