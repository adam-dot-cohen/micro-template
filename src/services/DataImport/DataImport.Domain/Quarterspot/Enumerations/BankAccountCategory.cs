using System;
using System.Collections.Generic;

namespace Laso.DataImport.Domain.Quarterspot.Enumerations
{
    public class BankAccountCategory : Enumeration<BankAccountCategory>
    {
        public static readonly BankAccountCategory Unknown = new BankAccountCategory(BankAccountCategoryValue.Unknown, "Unknown", AccountClass.Unknown);

        public static readonly BankAccountCategory AffinityRewards = new BankAccountCategory(BankAccountCategoryValue.AffinityRewards, "Affinity Rewards", AccountClass.Rewards);
        public static readonly BankAccountCategory AirlineRewards = new BankAccountCategory(BankAccountCategoryValue.AirlineRewards, "Airline Rewards", AccountClass.Rewards);
        public static readonly BankAccountCategory AutoRewards = new BankAccountCategory(BankAccountCategoryValue.AutoRewards, "Auto Rewards", AccountClass.Rewards);
        public static readonly BankAccountCategory HotelRewards = new BankAccountCategory(BankAccountCategoryValue.HotelRewards, "Hotel Rewards", AccountClass.Rewards);
        public static readonly BankAccountCategory ShoppingRewards = new BankAccountCategory(BankAccountCategoryValue.ShoppingRewards, "Shopping Rewards", AccountClass.Rewards);
        public static readonly BankAccountCategory OtherRewards = new BankAccountCategory(BankAccountCategoryValue.OtherRewards, "Other Rewards", AccountClass.Rewards);

        public static readonly BankAccountCategory Checking = new BankAccountCategory(BankAccountCategoryValue.Checking, "Checking", AccountClass.Banking, keywords: new[] { "Checking" });
        public static readonly BankAccountCategory Savings = new BankAccountCategory(BankAccountCategoryValue.Savings, "Savings", AccountClass.Banking, keywords: new[] { "Savings" });
        public static readonly BankAccountCategory MoneyMarket = new BankAccountCategory(BankAccountCategoryValue.MoneyMarket, "Money Market", AccountClass.Banking, keywords: new[] { "MoneyMarket" });
        public static readonly BankAccountCategory RecurringDeposit = new BankAccountCategory(BankAccountCategoryValue.RecurringDeposit, "Recurring Deposit", AccountClass.Banking);
        public static readonly BankAccountCategory CD = new BankAccountCategory(BankAccountCategoryValue.CD, "CD", AccountClass.Banking);
        public static readonly BankAccountCategory CashManagement = new BankAccountCategory(BankAccountCategoryValue.CashManagement, "Cash Management", AccountClass.Banking);
        public static readonly BankAccountCategory Overdraft = new BankAccountCategory(BankAccountCategoryValue.Overdraft, "Overdraft", AccountClass.Banking);
        public static readonly BankAccountCategory OtherBanking = new BankAccountCategory(BankAccountCategoryValue.OtherBanking, "Other Banking", AccountClass.Banking);

        public static readonly BankAccountCategory CreditCard = new BankAccountCategory(BankAccountCategoryValue.CreditCard, "Credit Card", AccountClass.Credit);
        public static readonly BankAccountCategory LineOfCredit = new BankAccountCategory(BankAccountCategoryValue.LineOfCredit, "Line of Credit", AccountClass.Credit, keywords: new[] { "CreditLine" });
        public static readonly BankAccountCategory OtherCredit = new BankAccountCategory(BankAccountCategoryValue.OtherCredit, "Other Credit", AccountClass.Credit);

        public static readonly BankAccountCategory TaxableInvestment = new BankAccountCategory(BankAccountCategoryValue.TaxableInvestment, "Taxable Investment", AccountClass.Investment);
        public static readonly BankAccountCategory Retirement401K = new BankAccountCategory(BankAccountCategoryValue.Retirement401K, "401K", AccountClass.Investment);
        public static readonly BankAccountCategory Brokerage = new BankAccountCategory(BankAccountCategoryValue.Brokerage, "Brokerage", AccountClass.Investment);
        public static readonly BankAccountCategory IRA = new BankAccountCategory(BankAccountCategoryValue.IRA, "IRA", AccountClass.Investment);
        public static readonly BankAccountCategory Retirement403B = new BankAccountCategory(BankAccountCategoryValue.Retirement403B, "403B", AccountClass.Investment);
        public static readonly BankAccountCategory Keogh = new BankAccountCategory(BankAccountCategoryValue.Keogh, "Keogh", AccountClass.Investment);
        public static readonly BankAccountCategory Trust = new BankAccountCategory(BankAccountCategoryValue.Trust, "Trust", AccountClass.Investment);
        public static readonly BankAccountCategory TDA = new BankAccountCategory(BankAccountCategoryValue.TDA, "TDA", AccountClass.Investment);
        public static readonly BankAccountCategory SIMPLE = new BankAccountCategory(BankAccountCategoryValue.SIMPLE, "SIMPLE", AccountClass.Investment);
        public static readonly BankAccountCategory NORMAL = new BankAccountCategory(BankAccountCategoryValue.NORMAL, "NORMAL", AccountClass.Investment);
        public static readonly BankAccountCategory SARSEP = new BankAccountCategory(BankAccountCategoryValue.SARSEP, "SARSEP", AccountClass.Investment);
        public static readonly BankAccountCategory UGMA = new BankAccountCategory(BankAccountCategoryValue.UGMA, "UGMA", AccountClass.Investment);
        public static readonly BankAccountCategory OtherInvestment = new BankAccountCategory(BankAccountCategoryValue.OtherInvestment, "Other Investment", AccountClass.Investment);

        public static readonly BankAccountCategory Loan = new BankAccountCategory(BankAccountCategoryValue.Loan, "Loan", AccountClass.Loan);
        public static readonly BankAccountCategory AutoLoan = new BankAccountCategory(BankAccountCategoryValue.AutoLoan, "Auto Loan", AccountClass.Loan);
        public static readonly BankAccountCategory CommercialLoan = new BankAccountCategory(BankAccountCategoryValue.CommercialLoan, "Commercial Loan", AccountClass.Loan);
        public static readonly BankAccountCategory ConstructionLoan = new BankAccountCategory(BankAccountCategoryValue.ConstructionLoan, "Construction Loan", AccountClass.Loan);
        public static readonly BankAccountCategory ConsumerLoan = new BankAccountCategory(BankAccountCategoryValue.ConsumerLoan, "Consumer Loan", AccountClass.Loan);
        public static readonly BankAccountCategory HomeEquityLoan = new BankAccountCategory(BankAccountCategoryValue.HomeEquityLoan, "Home Equity Loan", AccountClass.Loan);
        public static readonly BankAccountCategory MilitaryLoan = new BankAccountCategory(BankAccountCategoryValue.MilitaryLoan, "Military Loan", AccountClass.Loan);
        public static readonly BankAccountCategory MortgageLoan = new BankAccountCategory(BankAccountCategoryValue.MortgageLoan, "Mortgage Loan", AccountClass.Loan);
        public static readonly BankAccountCategory SmallMediumBusinessLoan = new BankAccountCategory(BankAccountCategoryValue.SmallMediumBusinessLoan, "Small/Medium Business Loan", AccountClass.Loan);
        public static readonly BankAccountCategory StudentLoan = new BankAccountCategory(BankAccountCategoryValue.StudentLoan, "Student Loan", AccountClass.Loan);
        public static readonly BankAccountCategory OtherLoan = new BankAccountCategory(BankAccountCategoryValue.OtherLoan, "Other Loan", AccountClass.Loan);

        public AccountClass AccountClass { get; private set; }
        public IEnumerable<string> Keywords { get; set; }

        private BankAccountCategory(int value, string displayName, AccountClass accountClass, IEnumerable<string> keywords = null)
            : base(value, displayName)
        {
            AccountClass = accountClass;
        }
    }

    public static class BankAccountCategoryValue
    {
        public const int Unknown = 0;

        public const int AffinityRewards = 1;
        public const int AirlineRewards = 2;
        public const int AutoRewards = 3;
        public const int HotelRewards = 4;
        public const int ShoppingRewards = 5;
        public const int OtherRewards = 6;

        public const int Checking = 10;
        public const int Savings = 11;
        public const int MoneyMarket = 12;
        public const int RecurringDeposit = 13;
        public const int CD = 14;
        public const int CashManagement = 15;
        public const int Overdraft = 16;
        public const int OtherBanking = 17;

        public const int CreditCard = 20;
        public const int LineOfCredit = 21;
        public const int OtherCredit = 22;

        public const int TaxableInvestment = 31;
        public const int Retirement401K = 32;
        public const int Brokerage = 33;
        public const int IRA = 34;
        public const int Retirement403B = 35;
        public const int Keogh = 36;
        public const int Trust = 37;
        public const int TDA = 38;
        public const int SIMPLE = 39;
        public const int NORMAL = 40;
        public const int SARSEP = 41;
        public const int UGMA = 42;
        public const int OtherInvestment = 43;

        public const int Loan = 50;
        public const int AutoLoan = 51;
        public const int CommercialLoan = 52;
        public const int ConstructionLoan = 53;
        public const int ConsumerLoan = 54;
        public const int HomeEquityLoan = 55;
        public const int MilitaryLoan = 56;
        public const int MortgageLoan = 57;
        public const int SmallMediumBusinessLoan = 58;
        public const int StudentLoan = 59;
        public const int OtherLoan = 60;
    }

    public enum AccountClass
    {
        Unknown = 0,

        Rewards = 1,
        Banking = 2,
        Credit = 3,
        Investment = 4,
        Loan = 5
    }
}
