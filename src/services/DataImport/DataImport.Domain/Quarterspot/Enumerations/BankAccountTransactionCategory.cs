using System;
using System.Collections.Generic;
using System.Text;

namespace Laso.DataImport.Domain.Quarterspot.Enumerations
{
    public enum CategoryType
    {
        Deposit = 1,
        Disbursement = 2,
        Uncategorized = 3
    }

    public enum DepositClassification
    {
        Revenues = 1,
        GrossUp = 2,
        OneTimeDeposit = 3,
        MultiAccountGrossUp = 4,
        PersonalIncome = 5,
        DoesNotApply = 6
    }

    public class BankAccountTransactionCategoryValue
    {
        // Common
        public static readonly int Uncategorized = 0;

        // Credits
        public static readonly int DepositWireIn = 1;
        public static readonly int DepositBankDeposit = 2;
        public static readonly int DepositPaypal = 3;
        public static readonly int DepositAmazon = 4;
        public static readonly int DepositCreditCard = 5;
        public static readonly int DepositAchDeposit = 6;
        public static readonly int DepositCreditCardReturn = 7;
        public static readonly int DepositBankFeeReturn = 8;
        public static readonly int DepositTaxRefund = 9;
        public static readonly int DepositLoan = 10;
        public static readonly int DepositInterbankTransferCredit = 11;
        public static readonly int DepositPayroll = 12;
        public static readonly int Deposit = 30;
        public static readonly int DepositOther = 31;
        public static readonly int DepositReturnItemCredit = 36;
        public static readonly int DepositWireReturn = 37;

        // Credits - V2 additions
        public static readonly int DepositAtm = 39;
        public static readonly int DepositAtmCash = 40;
        public static readonly int DepositAtmCheck = 41;
        public static readonly int DepositCash = 42;
        public static readonly int DepositCheck = 43;
        public static readonly int DepositOverdraft = 44;
        public static readonly int DepositRemoteMobile = 45;
        public static readonly int DepositTeller = 46;
        public static readonly int DepositTransfer = 47;
        public static readonly int DepositFeeRefund = 48;
        public static readonly int DepositHealthcareClaim = 49;
        public static readonly int DepositInsuranceDeposit = 50;
        public static readonly int DepositLoanProceeds = 51;
        public static readonly int DepositLoanRefund = 52;
        public static readonly int DepositOnlinePaymentProcessor = 53;
        public static readonly int DepositPaymentProcessor = 54;
        public static readonly int DepositQsLoanProceeds = 55;
        public static readonly int DepositQsLoanRefund = 56;

        public static readonly int DepositCorrection = 100;

        // Debits
        public static readonly int DisbursementsTaxPayment = 13;
        public static readonly int DisbursementsLoanPayment = 14;
        public static readonly int DisbursementsPayrollExpense = 15;
        public static readonly int DisbursementsEntertainmentMeals = 16;
        public static readonly int DisbursementsGasStations = 17;
        public static readonly int DisbursementsRecreationOther = 18;
        public static readonly int DisbursementsInterbankTransferDebit = 19;
        public static readonly int DisbursementsNsf = 20;
        public static readonly int DisbursementsBankFee = 21;
        public static readonly int DisbursementsOperatingExpenses = 22;
        public static readonly int DisbursementsCreditCardPayments = 23;
        public static readonly int DisbursementsUtilities = 24;
        public static readonly int DisbursementsCashWithdrawal = 25;
        public static readonly int DisbursementsChecks = 26;
        public static readonly int DisbursementsBankCardFees = 27;
        public static readonly int DisbursementsWireOut = 28;
        public static readonly int DisbursementsTravel = 29;
        public static readonly int DisbursementsChargeback = 32;
        public static readonly int DisbursementsReturnItemDebit = 33;
        public static readonly int DisbursementsLeaseOrRent = 34;
        public static readonly int DisbursementsChargecardPayments = 35;
        public static readonly int DisbursementsAchPayment = 38;

        // Debits - V2 Additions
        public static readonly int DisbursementsOtherWithdrawal = 57;
        public static readonly int DisbursementsAtmWithdrawal = 58;
        public static readonly int DisbursementsVehicleLoanPayment = 59;
        public static readonly int DisbursementsOnlinePayment = 60;
        public static readonly int DisbursementsQsLoanPayment = 61;
        public static readonly int DisbursementsUnknown = 62;
        public static readonly int DisbursementsCasino = 63;
        public static readonly int DisbursementsAtmFee = 64;
        public static readonly int DisbursementsGroceryStores = 65;
        public static readonly int DisbursementsWireFee = 66;
        public static readonly int DisbursementsPosPurchase = 67;
        public static readonly int DisbursementsOnlineShopping = 68;
        public static readonly int DisbursementsRestaurant = 69;
        public static readonly int DisbursementsDrugStore = 70;
        public static readonly int DisbursementsReturnedDeposit = 71;
        public static readonly int DisbursementsOfficeSupply = 72;
        public static readonly int DisbursementsPostalShipping = 73;
        public static readonly int DisbursementsOverdraftFee = 74;
        public static readonly int DisbursementsSuperStores = 75;
        public static readonly int DisbursementsHardware = 76;
        public static readonly int DisbursementsInsurance = 77;
        public static readonly int DisbursementsAdvertising = 78;
        public static readonly int DisbursementsOtherRetail = 79;
        public static readonly int DisbursementsServiceFee = 80;
        public static readonly int DisbursementsBillPay = 81;
        public static readonly int DisbursementsMortgagePayment = 82;
        public static readonly int DisbursementsIntuit = 83;
        public static readonly int DisbursementsChildSupport = 84;
        public static readonly int DisbursementsWithdrawalTeller = 85;
        public static readonly int DisbursementsInternationalWire = 86;
        public static readonly int DisbursementsStopPaymentFee = 87;
        public static readonly int DisbursementsOverdraftWithdrawal = 88;
        public static readonly int DisbursementsStudentLoanPayment = 89;
        public static readonly int DisbursementsCorporateTurnaround = 90;
        public static readonly int DisbursementsCreditRepair = 91;
        public static readonly int DisbursementsChargebackFee = 92;
        public static readonly int DisbursementsBailBonds = 93;
        public static readonly int DisbursementsWireFeeIntl = 94;

        public static readonly int DisbursementsInvestorWithdrawal = 95;
        public static readonly int DisbursementsLoanRefund = 96;
        public static readonly int DisbursementsAffiliatePayment = 97;
        public static readonly int DisbursementsCorrection = 99;
    }

    public class BankAccountTransactionCategory : Enumeration<BankAccountTransactionCategory>
    {
        public static readonly BankAccountTransactionCategory Uncategorized = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.Uncategorized, "Uncategorized", CategoryType.Uncategorized);

        // V1 Deposits
        public static readonly BankAccountTransactionCategory DepositWire = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositWireIn, "Wire In", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositBank = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositBankDeposit, "Bank Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositPaypal = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositPaypal, "Paypal", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositAmazon = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositAmazon, "Amazon", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositCreditCard = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositCreditCard, "Credit Card", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositAch = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositAchDeposit, "Ach Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositCreditCardReturn = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositCreditCardReturn, "Credit Card Return", CategoryType.Deposit, DepositClassification.GrossUp);
        public static readonly BankAccountTransactionCategory DepositBankFeeRefund = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositBankFeeReturn, "Bank Fee Return", CategoryType.Deposit, DepositClassification.GrossUp);
        public static readonly BankAccountTransactionCategory DepositTaxRefund = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositTaxRefund, "Tax Refund", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositLoan = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositLoan, "Loan", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositInterbankTransfer = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositInterbankTransferCredit, "Interbank Transfer", CategoryType.Deposit, DepositClassification.MultiAccountGrossUp);
        public static readonly BankAccountTransactionCategory DepositPayroll = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositPayroll, "Payroll", CategoryType.Deposit, DepositClassification.PersonalIncome);
        public static readonly BankAccountTransactionCategory Deposit = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.Deposit, "Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositOther = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositOther, "Other", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositReturnItem = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositReturnItemCredit, "Return Item", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositWireReturn = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositWireReturn, "Wire Return", CategoryType.Deposit, DepositClassification.Revenues);

        // V2 Additions
        public static readonly BankAccountTransactionCategory DepositAtm = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositAtm, "ATM Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositAtmCash = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositAtmCash, "ATM Cash Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositAtmCheck = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositAtmCheck, "ATM Check Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositCash = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositCash, "Cash Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositCheck = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositCheck, "Check Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositOverdraft = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositOverdraft, "Deposit Overdraft", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositRemoteMobile = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositRemoteMobile, "Remote / Mobile Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositTeller = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositTeller, "Teller Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositTransfer = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositTransfer, "Transfer Deposit", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositFeeRefund = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositFeeRefund, "Fee Refund", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositHealthcareClaim = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositHealthcareClaim, "Healthcare Claim", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositInsuranceDeposit = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositInsuranceDeposit, "Insurance Deposit", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositLoanProceeds = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositLoanProceeds, "Loan Proceeds", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositLoanRefund = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositLoanRefund, "Loan Refund", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositOnlinePaymentProcessor = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositOnlinePaymentProcessor, "Online Payment Processor", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositPaymentProcessor = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositPaymentProcessor, "Payment Processor", CategoryType.Deposit, DepositClassification.Revenues);
        public static readonly BankAccountTransactionCategory DepositQsLoanProceeds = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositQsLoanProceeds, "QuarterSpot Loan Proceeds", CategoryType.Deposit, DepositClassification.OneTimeDeposit);
        public static readonly BankAccountTransactionCategory DepositQsLoanRefund = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositQsLoanRefund, "QuarterSpot Loan Refund", CategoryType.Deposit, DepositClassification.OneTimeDeposit);

        public static readonly BankAccountTransactionCategory DepositCorrection = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DepositCorrection, "Correction", CategoryType.Deposit, DepositClassification.Revenues);

        // V1 Disbursements
        public static readonly BankAccountTransactionCategory DisbursementsTaxPayments = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsTaxPayment, "Tax Payment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsLoanPayments = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsLoanPayment, "Loan Payment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsPayrollExpense = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsPayrollExpense, "Payroll Expense", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsEntertainmentMeals = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsEntertainmentMeals, "Entertainment Meals", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsGasStations = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsGasStations, "Gas Stations", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsRecreationOther = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsRecreationOther, "Recreation Other", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsInterbankTransfer = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsInterbankTransferDebit, "Interbank Transfer", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsInsufficientFunds = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsNsf, "Non-sufficient Funds", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsBankFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsBankFee, "Bank Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOperatingExpenses = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOperatingExpenses, "Operating Expenses", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsCreditCardPayments = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsCreditCardPayments, "Credit Card Payments", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsUtilities = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsUtilities, "Utilities", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsCashWithdrawal = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsCashWithdrawal, "Cash Withdrawal", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsChecks = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsChecks, "Checks", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsBankCardFees = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsBankCardFees, "Bank Card Fees", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsWire = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsWireOut, "Wire Out", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsTravel = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsTravel, "Travel", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsChargeback = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsChargeback, "Chargeback", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsReturnItem = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsReturnItemDebit, "Return Item", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsLeaseRent = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsLeaseOrRent, "Lease/Rent", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsChargeCardPayments = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsChargecardPayments, "Chargecard Payments", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsAchPayments = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsAchPayment, "Ach Payment", CategoryType.Disbursement);

        // Disbursements - V2 Additions
        public static readonly BankAccountTransactionCategory DisbursementsOtherWithdrawal = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOtherWithdrawal, "Other Withdrawal", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsAtmWithdrawal = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsAtmWithdrawal, "Atm Withdrawal", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsVehicleLoanPayment = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsVehicleLoanPayment, "Vehicle Loan Payment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOnlinePayment = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOnlinePayment, "OnlinePayment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsQsLoanPayment = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsQsLoanPayment, "QsLoanPayment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsUnknown = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsUnknown, "Unknown", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsCasino = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsCasino, "Casino", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsAtmFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsAtmFee, "Atm Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsGroceryStores = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsGroceryStores, "Grocery Stores", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsWireFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsWireFee, "Wire Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsPosPurchase = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsPosPurchase, "Pos Purchase", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOnlineShopping = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOnlineShopping, "Online Shopping", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsRestaurant = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsRestaurant, "Restaurant", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsDrugStore = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsDrugStore, "Drug Store", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsReturnedDeposit = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsReturnedDeposit, "Returned Deposit", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOfficeSupply = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOfficeSupply, "Office Supply", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsPostalShipping = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsPostalShipping, "Postal / Shipping", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOverdraftFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOverdraftFee, "Overdraft Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsSuperStores = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsSuperStores, "Super Stores", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsHardware = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsHardware, "Hardware", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsInsurance = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsInsurance, "Insurance", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsAdvertising = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsAdvertising, "Advertising", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOtherRetail = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOtherRetail, "Other Retail", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsServiceFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsServiceFee, "Service Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsBillPay = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsBillPay, "Bill Pay", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsMortgagePayment = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsMortgagePayment, "Mortgage Payment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsIntuit = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsIntuit, "Intuit", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsChildSupport = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsChildSupport, "Child Support", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsWithdrawalTeller = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsWithdrawalTeller, "Withdrawal Teller", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsInternationalWire = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsInternationalWire, "International Wire", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsStopPaymentFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsStopPaymentFee, "Stop Payment Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsOverdraftWithdrawal = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsOverdraftWithdrawal, "Overdraft Withdrawal", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsStudentLoanPayment = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsStudentLoanPayment, "Student Loan Payment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsCorporateTurnaround = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsCorporateTurnaround, "Corporate Turnaround", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsCreditRepair = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsCreditRepair, "Credit Repair", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsChargebackFee = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsChargebackFee, "Chargeback Fee", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsBailBonds = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsBailBonds, "Bail Bonds", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsWireFeeIntl = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsWireFeeIntl, "Wire Fee Intl", CategoryType.Disbursement);

        public static readonly BankAccountTransactionCategory DisbursementsInvestorWithdrawal = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsInvestorWithdrawal, "Investor Withdrawal", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsLoanRefund = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsLoanRefund, "Loan Refund", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsAffiliatePayment = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsAffiliatePayment, "Affiliate Payment", CategoryType.Disbursement);
        public static readonly BankAccountTransactionCategory DisbursementsCorrection = new BankAccountTransactionCategory(BankAccountTransactionCategoryValue.DisbursementsCorrection, "Correction", CategoryType.Disbursement);


        private BankAccountTransactionCategory(int value, string displayName, CategoryType categoryType, DepositClassification classification = DepositClassification.DoesNotApply)
            : base(value, displayName)
        {
            CategoryType = categoryType;
            Classification = classification;
        }

        public CategoryType CategoryType { get; }
        public DepositClassification Classification { get; }
    }
}
