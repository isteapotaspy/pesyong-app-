using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs
{
    /// <summary>
    /// Represents a single item included in the checkout request.
    /// </summary>
    public class CheckoutItemDto
    {
        /// <summary>
        /// Gets or sets the product ID of the item being checked out.
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        /// Gets or sets the quantity of the selected product.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price of the item at the time of checkout.
        /// </summary>
        public decimal ItemPrice { get; set; }

        /// <summary>
        /// Gets or sets the product type, such as meal, catering package, or kakanin.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected catering meal contents when the item is a catering package.
        /// </summary>
        public List<CateringCartSelectionDto>? CateringSelections { get; set; }
    }
}