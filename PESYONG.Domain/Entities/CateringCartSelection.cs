using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities
{
    public class CateringCartSelection
    {
        public int MealId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
