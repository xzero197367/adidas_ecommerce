using Adidas.Models.Main;
using Models.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Main
{
    public class UserProductView 
    {
        public Guid Id { get; set; }
        public  string UserId { get; set; }
        public Guid ProductId { get; set; }  
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Product Product { get; set; }
    }
}
