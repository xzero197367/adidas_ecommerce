
using System.ComponentModel.DataAnnotations;


namespace Adidas.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
