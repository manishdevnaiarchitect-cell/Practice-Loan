using LoanProcessingApp.Controllers;
using LoanProcessingApp.Dtos;
using LoanProcessingApp.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LoanProcessingApp.Tests
{
    public class LoanApplicationsControllerTests
    {
        private readonly Mock<ILoanApplicationService> _serviceMock;
        private readonly LoanApplicationsController _controller;

        public LoanApplicationsControllerTests()
        {
            _serviceMock = new Mock<ILoanApplicationService>();
            _controller = new LoanApplicationsController(_serviceMock.Object);
        }

        [Fact]
        public void Submit_ValidRequest_ReturnsOkWithApprovedResponse()
        {
            // Arrange
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-001",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 100000m,
                TenureMonths = 60
            };

            var expectedResponse = new LoanApplicationResponse
            {
                CustomerId = "CUST-001",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 100000m,
                TenureMonths = 60,
                IsApproved = true,
                Status = "Approved",
                MaxEligibleLoanAmount = 300000m,
                Message = "Loan application approved."
            };

            _serviceMock
                .Setup(s => s.Submit(request))
                .Returns(expectedResponse);

            // Act
            var actionResult = _controller.Submit(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<LoanApplicationResponse>(okResult.Value);
            Assert.True(response.IsApproved);
            Assert.Equal("Approved", response.Status);
            _serviceMock.Verify(s => s.Submit(request), Times.Once);
        }

        [Fact]
        public void Submit_ServiceReturnsRejected_ReturnsOkWithRejectedResponse()
        {
            // Arrange
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-002",
                AnnualIncome = 20000m,
                RequestedLoanAmount = 150000m,
                TenureMonths = 24
            };

            var expectedResponse = new LoanApplicationResponse
            {
                CustomerId = "CUST-002",
                Status = "Rejected",
                IsApproved = false,
                MaxEligibleLoanAmount = 100000m,
                Message = "Requested amount exceeds maximum eligible amount of $100,000.00."
            };

            _serviceMock
                .Setup(s => s.Submit(request))
                .Returns(expectedResponse);

            // Act
            var actionResult = _controller.Submit(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<LoanApplicationResponse>(okResult.Value);
            Assert.False(response.IsApproved);
            Assert.Equal("Rejected", response.Status);
        }

        [Fact]
        public void Submit_ServiceReturnsInvalidStatus_ReturnsBadRequest()
        {
            // Arrange
            var request = new LoanApplicationRequest
            {
                CustomerId = "",
                AnnualIncome = 60000m,
                RequestedLoanAmount = 50000m,
                TenureMonths = 24
            };

            var invalidResponse = new LoanApplicationResponse
            {
                CustomerId = "",
                Status = "Invalid",
                IsApproved = false,
                Message = "Customer ID cannot be empty or whitespace."
            };

            _serviceMock
                .Setup(s => s.Submit(request))
                .Returns(invalidResponse);

            // Act
            var actionResult = _controller.Submit(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            var response = Assert.IsType<LoanApplicationResponse>(badRequestResult.Value);
            Assert.Equal("Invalid", response.Status);
        }

        [Fact]
        public void Submit_ModelStateInvalid_ReturnsValidationProblemAndDoesNotCallService()
        {
            // Arrange
            var request = new LoanApplicationRequest
            {
                CustomerId = null,
                AnnualIncome = 0m,
                RequestedLoanAmount = 0m,
                TenureMonths = 0
            };

            _controller.ModelState.AddModelError("CustomerId", "Customer ID is required.");

            // Act
            var actionResult = _controller.Submit(request);

            // Assert
            Assert.IsType<ObjectResult>(actionResult.Result);
            _serviceMock.Verify(s => s.Submit(It.IsAny<LoanApplicationRequest>()), Times.Never);
        }

        [Fact]
        public void Submit_CallsServiceExactlyOnceWithGivenRequest()
        {
            // Arrange
            var request = new LoanApplicationRequest
            {
                CustomerId = "CUST-003",
                AnnualIncome = 45000m,
                RequestedLoanAmount = 30000m,
                TenureMonths = 12
            };

            _serviceMock
                .Setup(s => s.Submit(It.IsAny<LoanApplicationRequest>()))
                .Returns(new LoanApplicationResponse { Status = "Approved", IsApproved = true });

            // Act
            _controller.Submit(request);

            // Assert
            _serviceMock.Verify(s => s.Submit(request), Times.Once);
        }
    }
}
