using PESYONG.Domain.Entities.Orders;
using PESYONG.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Presentation.ViewModels
{
    // ViewModel for Order display
    public class OrderViewModel
    {
        private readonly Order _order;

        public OrderViewModel(Order order)
        {
            _order = order;
        }

        public Guid OrderID => _order.OrderID;
        public DateTime OrderDate => _order.OrderDate;
        public DateTime? ActualDeliveryDate => _order.ActualDeliveryDate;
        public DeliveryStatus DeliveryStatus => _order.DeliveryStatus;
        public ICollection<OrderMealProduct> OrderItems => _order.OrderItems;
        public decimal OrderTotalAmount => _order.OrderTotalAmount;
    }
}
