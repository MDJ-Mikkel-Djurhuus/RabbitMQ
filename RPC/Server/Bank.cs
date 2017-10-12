namespace Bank
{
    public class Loan
    {
        LoanRequest request;
        double loanInterestStandard = 5.0;

        public Loan(LoanRequest request)
        {
            this.request = request;
        }

        public LoanRespond getLoan()
        {
            double loanInterest = loanInterestStandard;
            if (request.LoanAmount < 250000)
            {
                loanInterest = 5.0;
            }
            else if (request.LoanAmount < 500000)
            {
                loanInterest = 4.0;
            }
            else if (request.LoanAmount < 1000000)
            {
                loanInterest = 3.0;
            }
            else
            {
                loanInterest = 2.0;
            }

            if (request.LoanDuration < 360)
            {
                loanInterest += 3;
            }
            else if (request.LoanDuration < 5 * 360)
            {
                loanInterest += 2;
            }
            else if (request.LoanDuration < 15 * 360)
            {
                loanInterest += 1;
            }
            return new LoanRespond(loanInterest, request.Ssn);
        }
    }

    public struct LoanRequest
    {
        private string ssn;
        public string Ssn { get => ssn; }
        private double loanAmount;
        public double LoanAmount { get => loanAmount; }
        private int loanDuration;
        public int LoanDuration { get => loanDuration; }
        private int creditScore;
        public int CreditScore { get => creditScore; }
        public LoanRequest(string ssn, double loanAmount, int loanDuration, int creditScore)
        {
            this.ssn = ssn;
            this.loanAmount = loanAmount;
            this.loanDuration = loanDuration;
            this.creditScore = creditScore;
        }

    }

    public struct LoanRespond
    {
        double interestRate;
        public double InterestRate { get => interestRate; }
        string ssn;
        public string Ssn { get => ssn; }
        public LoanRespond(double interestRate, string ssn)
        {
            this.interestRate = interestRate;
            this.ssn = ssn;
        }

    }
}