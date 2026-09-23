using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LoanProcessingApp.Dtos;

namespace LoanProcessingApp.Validation
{
    public interface ILoanApplicationValidator
    {
        LoanApplicationValidationResult Validate(LoanApplicationRequest request);
    }

    public class LoanApplicationValidationResult
    {
        public bool IsValid => !Errors.Any();

        public IReadOnlyList<string> Errors { get; }

        public LoanApplicationValidationResult(IReadOnlyList<string> errors)
        {
            Errors = errors ?? Array.Empty<string>();
        }
    }

    public class LoanApplicationValidator : ILoanApplicationValidator
    {
        private const decimal MinLoanAmount = 1000m;
        private const decimal MaxLoanAmount = 10_000_000m;
        private const int MinTenureMonths = 1;
        private const int MaxTenureMonths = 480;

        public LoanApplicationValidationResult Validate(LoanApplicationRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Loan application request cannot be null.");
                return new LoanApplicationValidationResult(errors);
            }

            // Run standard DataAnnotations validation first.
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
            {
                errors.AddRange(results.Select(r => r.ErrorMessage));
            }

            // Additional business rules.
            if (string.IsNullOrWhiteSpace(request.CustomerId))
            {
                errors.Add("Customer ID cannot be empty or whitespace.");
            }

            if (request.RequestedLoanAmount > 0 && request.RequestedLoanAmount < MinLoanAmount)
            {
                errors.Add($"Requested loan amount must be at least {MinLoanAmount:C}.");
            }

            if (request.RequestedLoanAmount > MaxLoanAmount)
            {
                errors.Add($"Requested loan amount cannot exceed {MaxLoanAmount:C}.");
            }

            if (request.TenureMonths < MinTenureMonths || request.TenureMonths > MaxTenureMonths)
            {
                errors.Add($"Tenure must be between {MinTenureMonths} and {MaxTenureMonths} months.");
            }

            if (request.AnnualIncome > 0 && request.RequestedLoanAmount > request.AnnualIncome * 10)
            {
                errors.Add("Requested loan amount cannot exceed 10 times the annual income.");
            }

            // De-duplicate errors that may have been raised by both DataAnnotations and business rules.
            var distinctErrors = errors.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();

            return new LoanApplicationValidationResult(distinctErrors);
        }
    }
}
