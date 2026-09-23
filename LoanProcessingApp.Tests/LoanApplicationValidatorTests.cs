using LoanProcessingApp.Dtos;
using LoanProcessingApp.Validation;
using Xunit;

namespace LoanProcessingApp.Tests
{
    public class LoanApplicationValidatorTests
    {
        private readonly LoanApplicationValidator _validator = new LoanApplicationValidator();

        [Fact]
        public void Validate_ValidRequest_ReturnsNoErrors()
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 100000m,
                TenureMonths = 60
            };

            var result = _validator.Validate(request);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Validate_NullRequest_ReturnsError()
        {
            var result = _validator.Validate(null);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("cannot be null"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_MissingCustomerId_ReturnsError(string customerId)
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = customerId,
                AnnualIncome = 60000m,
                RequestedLoanAmount = 50000m,
                TenureMonths = 24
            };

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Customer ID"));
        }

        [Fact]
        public void Validate_ZeroAnnualIncome_ReturnsError()
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 0m,
                RequestedLoanAmount = 50000m,
                TenureMonths = 24
            };

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Annual income"));
        }

        [Fact]
        public void Validate_LoanAmountBelowMinimum_ReturnsError()
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 500m,
                TenureMonths = 24
            };

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("at least"));
        }

        [Fact]
        public void Validate_LoanAmountAboveMaximum_ReturnsError()
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 6000000m,
                RequestedLoanAmount = 20_000_000m,
                TenureMonths = 24
            };

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("cannot exceed"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(481)]
        [InlineData(-1)]
        public void Validate_InvalidTenure_ReturnsError(int tenure)
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 50000m,
                TenureMonths = tenure
            };

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Tenure"));
        }

        [Fact]
        public void Validate_LoanAmountExceedsIncomeMultiplier_ReturnsError()
        {
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 20000m,
                RequestedLoanAmount = 500000m,
                TenureMonths = 36
            };

            var result = _validator.Validate(request);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("10 times"));
        }
    }
}
