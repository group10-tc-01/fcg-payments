using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi.Models
{
    [ExcludeFromCodeCoverage]
    public class DepositRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
}
