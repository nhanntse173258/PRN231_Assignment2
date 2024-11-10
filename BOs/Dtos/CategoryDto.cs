using BOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Dtos
{
    public class CategoryDto
    {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public string? FromCountry { get; set; }

        public virtual ICollection<SilverJewelryDto> SilverJewelries { get; set; } = new List<SilverJewelryDto>();
    }
}
