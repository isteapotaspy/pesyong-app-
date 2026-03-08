using PESYONG.Domain.Entities.Orders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PESYONG.Domain.Entities.Users;

public class Customer
{
    [Key]
    public Guid CustomerID { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; } //optional for guest checkout

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty; //required for delivery contact

    [StringLength(200)]
    public string? Address { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
