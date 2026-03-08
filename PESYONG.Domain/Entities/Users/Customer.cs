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
    public Guid CustomerID { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(200)]
    public string Address { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public bool IsValid()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();

        return Validator.TryValidateObject(this, validationContext, validationResults, validateAllProperties: true);
    }
    public IEnumerable<string> GetValidationErrors()
    {
        var validationContext = new ValidationContext(this);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(this, validationContext, validationResults, true);
        return validationResults.Select(vr => vr.ErrorMessage);
    }
}
