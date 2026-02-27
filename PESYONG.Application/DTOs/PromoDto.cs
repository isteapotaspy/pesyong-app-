using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Application.DTOs;
public record PromoCodeDto(int Id, string Code, decimal DiscountPercentage);