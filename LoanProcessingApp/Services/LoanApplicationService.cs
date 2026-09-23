using System;
using LoanProcessingApp.Dtos;
using LoanProcessingApp.Validation;

namespace LoanProcessingApp.Services
{
    public interface ILoanApplicationService
    {
        LoanApplicationResponse Submit(LoanApplicationRequest request);
    }

    /// <summary>
    /// Evaluates customer loan applications: validates input and applies
    /// a simple income-based eligibility rule.
    /// </summary>
    public class LoanApplicationService : ILoanApplicationService
    {
        private const decimal EligibilityIncomeMultiplier = 5m;

        private readonly ILoanApplicationValidator _validator;

        public LoanApplicationService(ILoanApplicationValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public LoanApplicationResponse Submit(LoanApplicationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var validationResult = _validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return new LoanApplicationResponse
                {
                    CustomerId = request.CustomerId,
                    AnnualIncome = request.AnnualIncome,
                    RequestedLoanAmount = request.RequestedLoanAmount,
                    TenureMonths = request.TenureMonths,
                    IsApproved = false,
                    Status = "Invalid",
                    Message = string.Join(" ", validationResult.Errors)
                };
            }

            decimal maxEligibleAmount = request.AnnualIncome * EligibilityIncomeMultiplier;
            bool isApproved = request.RequestedLoanAmount <= maxEligibleAmount;

            return new LoanApplicationResponse
            {
                CustomerId = request.CustomerId,
                AnnualIncome = request.AnnualIncome,
                RequestedLoanAmount = request.RequestedLoanAmount,
                TenureMonths = request.TenureMonths,
                IsApproved = isApproved,
                Status = isApproved ? "Approved" : "Rejected",
                MaxEligibleLoanAmount = maxEligibleAmount,
                Message = isApproved
                    ? "Loan application approved."
                    : $"Requested amount exceeds maximum eligible amount of {maxEligibleAmount:C}."
            };
        }
    }
}
