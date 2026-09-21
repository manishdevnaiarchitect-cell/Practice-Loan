```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace LoanProcessingApp
{
    public enum LoanType
    {
        Home,
        Personal,
        Auto,
        Education
    }

    public class LoanApplication
    {
        public int Id { get; set; }
        public string ApplicantName { get; set; }
        public string Email { get; set; }
        public decimal AnnualIncome { get; set; }
        public decimal LoanAmount { get; set; }
        public int CreditScore { get; set; }
        public LoanType Type { get; set; }
        public bool IsExistingCustomer { get; set; }
        public string Status { get; set; }
    }

    public class LoanProcessor
    {
        private readonly string connectionString =
            "Server=localhost;Database=Loans;User Id=sa;Password=Password123;";

        private readonly HttpClient httpClient = new HttpClient();

        public void ProcessLoan(LoanApplication loan)
        {
            Console.WriteLine("Starting loan processing...");

            // Validation
            if (loan == null)
            {
                Console.WriteLine("Loan application cannot be null");
                return;
            }

            if (string.IsNullOrEmpty(loan.ApplicantName))
            {
                Console.WriteLine("Applicant name is required");
                return;
            }

            if (string.IsNullOrEmpty(loan.Email))
            {
                Console.WriteLine("Email is required");
                return;
            }

            if (loan.AnnualIncome <= 0)
            {
                Console.WriteLine("Annual income must be greater than zero");
                return;
            }

            if (loan.LoanAmount <= 0)
            {
                Console.WriteLine("Loan amount must be greater than zero");
                return;
            }

            if (loan.CreditScore < 300 || loan.CreditScore > 900)
            {
                Console.WriteLine("Invalid credit score");
                return;
            }

            // Calculate debt-to-income ratio
            decimal monthlyIncome = loan.AnnualIncome / 12;
            decimal estimatedMonthlyPayment = loan.LoanAmount * 0.01m;
            decimal dti = estimatedMonthlyPayment / monthlyIncome;

            if (dti > 0.50m)
            {
                Console.WriteLine("DTI ratio too high");
                loan.Status = "Rejected";
                SaveLoan(loan);
                SendEmail(
                    loan.Email,
                    "Loan Rejected",
                    "Your loan application has been rejected because your DTI ratio is too high."
                );
                return;
            }

            // Credit score check
            if (loan.CreditScore < 650)
            {
                Console.WriteLine("Credit score too low");
                loan.Status = "Rejected";

                SaveLoan(loan);

                SendEmail(
                    loan.Email,
                    "Loan Rejected",
                    "Your loan application has been rejected because of your credit score."
                );

                return;
            }

            // External credit bureau call
            try
            {
                string url =
                    "https://creditbureau.example.com/api/check/" +
                    loan.ApplicantName;

                var response = httpClient.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Credit bureau unavailable");
                    loan.Status = "Manual Review";
                    SaveLoan(loan);
                    return;
                }

                string responseBody = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine("Credit bureau response:");
                Console.WriteLine(responseBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Credit bureau error: " + ex.Message);
                loan.Status = "Manual Review";
                SaveLoan(loan);
                return;
            }

            // Calculate interest rate
            decimal interestRate = 0;

            if (loan.Type == LoanType.Home)
            {
                interestRate = 7.5m;

                if (loan.IsExistingCustomer)
                {
                    interestRate -= 0.25m;
                }
            }
            else if (loan.Type == LoanType.Personal)
            {
                interestRate = 11.5m;

                if (loan.IsExistingCustomer)
                {
                    interestRate -= 0.50m;
                }
            }
            else if (loan.Type == LoanType.Auto)
            {
                interestRate = 9.25m;

                if (loan.IsExistingCustomer)
                {
                    interestRate -= 0.25m;
                }
            }
            else if (loan.Type == LoanType.Education)
            {
                interestRate = 6.5m;

                if (loan.IsExistingCustomer)
                {
                    interestRate -= 0.50m;
                }
            }
            else
            {
                interestRate = 15m;
            }

            // Calculate processing fee
            decimal processingFee = 0;

            if (loan.Type == LoanType.Home)
            {
                processingFee = loan.LoanAmount * 0.005m;
            }
            else if (loan.Type == LoanType.Personal)
            {
                processingFee = loan.LoanAmount * 0.02m;
            }
            else if (loan.Type == LoanType.Auto)
            {
                processingFee = loan.LoanAmount * 0.01m;
            }
            else if (loan.Type == LoanType.Education)
            {
                processingFee = loan.LoanAmount * 0.0025m;
            }

            // Calculate approval amount
            decimal maximumLoanAmount = 0;

            if (loan.Type == LoanType.Home)
            {
                maximumLoanAmount = loan.AnnualIncome * 5;
            }
            else if (loan.Type == LoanType.Personal)
            {
                maximumLoanAmount = loan.AnnualIncome * 2;
            }
            else if (loan.Type == LoanType.Auto)
            {
                maximumLoanAmount = loan.AnnualIncome * 3;
            }
            else if (loan.Type == LoanType.Education)
            {
                maximumLoanAmount = loan.AnnualIncome * 4;
            }

            if (loan.LoanAmount > maximumLoanAmount)
            {
                Console.WriteLine("Requested loan amount exceeds maximum limit");
                loan.Status = "Rejected";

                SaveLoan(loan);

                SendEmail(
                    loan.Email,
                    "Loan Rejected",
                    "Requested loan amount exceeds the permitted limit."
                );

                return;
            }

            // Fraud check
            if (loan.LoanAmount > 5000000)
            {
                Console.WriteLine("Large loan amount - fraud review required");
                loan.Status = "Manual Review";

                SaveLoan(loan);

                LogAudit(
                    loan,
                    "Loan moved to manual review because amount exceeds fraud threshold."
                );

                return;
            }

            // Approval
            loan.Status = "Approved";

            SaveLoan(loan);

            LogAudit(
                loan,
                "Loan approved. Interest Rate: " +
                interestRate +
                "%. Processing Fee: " +
                processingFee
            );

            SendEmail(
                loan.Email,
                "Loan Approved",
                "Congratulations. Your loan has been approved." +
                Environment.NewLine +
                "Interest Rate: " + interestRate + "%" +
                Environment.NewLine +
                "Processing Fee: " + processingFee
            );

            GenerateLoanDocument(
                loan,
                interestRate,
                processingFee
            );

            Console.WriteLine("Loan processing completed.");
        }

        private void SaveLoan(LoanApplication loan)
        {
            Console.WriteLine("Connecting to database...");

            Console.WriteLine(
                "INSERT INTO Loans VALUES (" +
                loan.Id + ", '" +
                loan.ApplicantName + "', '" +
                loan.Email + "', " +
                loan.AnnualIncome + ", " +
                loan.LoanAmount + ", " +
                loan.CreditScore + ", '" +
                loan.Type + "', '" +
                loan.Status + "')"
            );

            File.AppendAllText(
                "loan-audit.txt",
                DateTime.Now +
                " - Loan saved: " +
                loan.Id +
                Environment.NewLine
            );
        }

        private void SendEmail(
            string email,
            string subject,
            string message)
        {
            Console.WriteLine("Connecting to SMTP server...");
            Console.WriteLine("Sending email to " + email);
            Console.WriteLine("Subject: " + subject);
            Console.WriteLine(message);
        }

        private void LogAudit(
            LoanApplication loan,
            string message)
        {
            string auditMessage =
                DateTime.Now +
                " | LoanId=" +
                loan.Id +
                " | Applicant=" +
                loan.ApplicantName +
                " | " +
                message;

            Console.WriteLine(auditMessage);

            File.AppendAllText(
                "loan-audit.txt",
                auditMessage + Environment.NewLine
            );
        }

        private void GenerateLoanDocument(
            LoanApplication loan,
            decimal interestRate,
            decimal processingFee)
        {
            var document = new
            {
                LoanId = loan.Id,
                Applicant = loan.ApplicantName,
                Amount = loan.LoanAmount,
                InterestRate = interestRate,
                ProcessingFee = processingFee,
                Status = loan.Status,
                GeneratedAt = DateTime.Now
            };

            string json =
                JsonSerializer.Serialize(
                    document,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                "loan-" + loan.Id + ".json",
                json
            );

            Console.WriteLine(
                "Loan document generated."
            );
        }

        public void GenerateMonthlyStatement(
            LoanApplication loan)
        {
            Console.WriteLine(
                "Generating monthly statement..."
            );

            if (loan.Status != "Approved")
            {
                Console.WriteLine(
                    "Statement cannot be generated."
                );

                return;
            }

            decimal interestRate;

            if (loan.Type == LoanType.Home)
            {
                interestRate = 7.5m;
            }
            else if (loan.Type == LoanType.Personal)
            {
                interestRate = 11.5m;
            }
            else if (loan.Type == LoanType.Auto)
            {
                interestRate = 9.25m;
            }
            else
            {
                interestRate = 6.5m;
            }

            decimal monthlyInterest =
                loan.LoanAmount *
                (interestRate / 100) /
                12;

            Console.WriteLine(
                "Loan Statement"
            );

            Console.WriteLine(
                "Applicant: " +
                loan.ApplicantName
            );

            Console.WriteLine(
                "Loan Amount: " +
                loan.LoanAmount
            );

            Console.WriteLine(
                "Interest Rate: " +
                interestRate
            );

            Console.WriteLine(
                "Monthly Interest: " +
                monthlyInterest
            );

            SendEmail(
                loan.Email,
                "Monthly Loan Statement",
                "Your monthly interest is " +
                monthlyInterest
            );
        }
    }
}
```
