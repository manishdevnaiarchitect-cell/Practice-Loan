namespace LoanProcessingApp.Dtos
{
    /// <summary>
    /// Result returned after a loan application has been evaluated.
    /// </summary>
    public class LoanApplicationResponse
    {
        public string CustomerId { get; set; }

        public decimal AnnualIncome { get; set; }

        public decimal RequestedLoanAmount { get; set; }

        public int TenureMonths { get; set; }

        public bool IsApproved { get; set; }

        public string Status { get; set; }

        public decimal MaxEligibleLoanAmount { get; set; }

        public string Message { get; set; }
    }
}
