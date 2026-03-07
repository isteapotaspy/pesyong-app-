using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs
{
    public class CheckoutItemDto
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal ItemPrice { get; set; }
        public string Type { get; set; } = string.Empty;

        public List<CateringCartSelectionDto>? CateringSelections { get; set; }
    }
}
