using System.ComponentModel.DataAnnotations;

namespace LoanProcessingApp.Dtos
{
    /// <summary>
    /// Request payload submitted by a customer applying for a loan.
    /// </summary>
    public class LoanApplicationRequest
    {
        [Required(ErrorMessage = "Customer ID is required.")]
        public string CustomerId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Annual income must be greater than zero.")]
        public decimal AnnualIncome { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Requested loan amount must be greater than zero.")]
        public decimal RequestedLoanAmount { get; set; }

        [Range(1, 480, ErrorMessage = "Tenure must be between 1 and 480 months.")]
        public int TenureMonths { get; set; }
    }
}
