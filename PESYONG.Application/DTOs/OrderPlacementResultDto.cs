using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.ApplicationLogic.DTOs
{
    public class OrderPlacementResultDto
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
    }
}
