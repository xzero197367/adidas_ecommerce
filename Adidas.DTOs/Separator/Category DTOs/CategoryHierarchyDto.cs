using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adidas.DTOs.Separator.Category_DTOs
{
    public class CategoryHierarchyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int Level { get; set; }
    }
}
