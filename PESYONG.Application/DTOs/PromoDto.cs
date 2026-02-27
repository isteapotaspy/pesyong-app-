using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Application.DTOs;

/// <remarks>
/// Add this to another folder in /DTOs named "Display".
/// </remarks>

public record PromoCodeDto(int Id, string Code, decimal DiscountPercentage);