
using Adidas.Models;
using Adidas.Models.Main;
using Models.People;

namespace Models.Feature
{
 
    public class Wishlist: BaseAuditableEntity
    {   
        // fields
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        // forieng keys
        public required string UserId { get; set; }
        public required Guid ProductId { get; set; }
        // navigation properties
        public User User { get; set; }
        public Product Product { get; set; }
    }
}
