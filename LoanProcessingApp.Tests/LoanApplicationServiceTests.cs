using LoanProcessingApp.Dtos;
using LoanProcessingApp.Services;
using LoanProcessingApp.Validation;
using Moq;
using Xunit;

namespace LoanProcessingApp.Tests
{
    public class LoanApplicationServiceTests
    {
        private readonly Mock<ILoanApplicationValidator> _validatorMock;
        private readonly LoanApplicationService _service;

        public LoanApplicationServiceTests()
        {
            _validatorMock = new Mock<ILoanApplicationValidator>();
            _service = new LoanApplicationService(_validatorMock.Object);
        }

        [Fact]
        public void Submit_ValidRequestWithinLimit_ReturnsApproved()
        {
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LoanApplicationRequest>()))
                .Returns(new LoanApplicationValidationResult(new string[0]));

            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 100000m, // <= 5x income (300000)
                TenureMonths = 60
            };

            var response = _service.Submit(request);

            Assert.True(response.IsApproved);
            Assert.Equal("Approved", response.Status);
            Assert.Equal(300000m, response.MaxEligibleLoanAmount);
        }

        [Fact]
        public void Submit_ValidRequestExceedingLimit_ReturnsRejected()
        {
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LoanApplicationRequest>()))
                .Returns(new LoanApplicationValidationResult(new string[0]));

            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 20000m,
                RequestedLoanAmount = 150000m, // > 5x income (100000)
                TenureMonths = 24
            };

            var response = _service.Submit(request);

            Assert.False(response.IsApproved);
            Assert.Equal("Rejected", response.Status);
            Assert.Equal(100000m, response.MaxEligibleLoanAmount);
        }

        [Fact]
        public void Submit_InvalidRequest_ReturnsInvalidStatusWithMessage()
        {
            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LoanApplicationRequest>()))
                .Returns(new LoanApplicationValidationResult(new[] { "Customer ID cannot be empty or whitespace." }));

            var request = new LoanApplicationRequest
            {
                CustomerId = "",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 50000m,
                TenureMonths = 24
            };

            var response = _service.Submit(request);

            Assert.False(response.IsApproved);
            Assert.Equal("Invalid", response.Status);
            Assert.Contains("Customer ID", response.Message);
        }

        [Fact]
        public void Submit_NullRequest_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => _service.Submit(null));
        }
    }
}
