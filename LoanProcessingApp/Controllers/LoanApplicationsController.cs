using Microsoft.AspNetCore.Mvc;
using LoanProcessingApp.Dtos;
using LoanProcessingApp.Services;

namespace LoanProcessingApp.Controllers
{
    [ApiController]
    [Route("api/loan-applications")]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanApplicationService _loanApplicationService;

        public LoanApplicationsController(ILoanApplicationService loanApplicationService)
        {
            _loanApplicationService = loanApplicationService;
        }

        /// <summary>
        /// Submits a new customer loan application for evaluation.
        /// </summary>
        [HttpPost]
        public ActionResult<LoanApplicationResponse> Submit([FromBody] LoanApplicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var response = _loanApplicationService.Submit(request);

            if (response.Status == "Invalid")
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
