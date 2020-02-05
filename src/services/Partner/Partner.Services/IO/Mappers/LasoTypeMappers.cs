using System;
using System.Collections.Generic;
using CsvHelper.Configuration;
using Partner.Domain.Laso;
using Partner.Domain.Laso.Models;

namespace Partner.Services.IO.Mappers
{
    public class LasoDemographicsMapper : ClassMap<Demographic>
    {
        public LasoDemographicsMapper()
        {
            CsvHelperMapper.MapAll<Demographic>(this);
        }
    }

    public class LasoFirmographicsMapper : ClassMap<Firmographic>
    {
        public LasoFirmographicsMapper()
        {            
            CsvHelperMapper.MapAll<Firmographic>(this, new Dictionary<string, string>
            {
                [nameof(Firmographic.IndustryNaics)] = "Industry_NAICS",
                [nameof(Firmographic.IndustrySic)] = "Industry_SIC",
                [nameof(Firmographic.BusinessEin)] = "EIN"
            });
        }
    }

    public class LasoAccountMapper : ClassMap<Account>
    {
        public LasoAccountMapper()
        {            
            CsvHelperMapper.MapAll<Account>(this);
        }
    }

    public class LasoAccountTransactionMapper : ClassMap<AccountTransaction>
    {
        public LasoAccountTransactionMapper()
        {
            CsvHelperMapper.MapAll<AccountTransaction>(this, new Dictionary<string, string>
            {
                [nameof(AccountTransaction.MccCode)] = "MCC_Code"
            });
        }
    }

    public class LasoLoanAccountMapper : ClassMap<LoanAccount>
    {
        public LasoLoanAccountMapper()
        {
            CsvHelperMapper.MapAll<LoanAccount>(this, new Dictionary<string, string>
            {
                [nameof(LoanAccount.RefinanceLoanId)] = "Refinance_Loan_ID",
                [nameof(LoanAccount.PastDue01To29DaysCount)] = "Past_Due_01_to_29_Days_Count",
                [nameof(LoanAccount.PastDue30To59DaysCount)] = "Past_Due_30_to_59_Days_Count",
                [nameof(LoanAccount.PastDue60To89DaysCount)] = "Past_Due_60_to_89_Days_Count",
                [nameof(LoanAccount.PastDueOver90DaysCount)] = "Past_Due_Over_90_Days_Count"
            });
        }
    }

    public class LasoLoanCollateralMapper : ClassMap<LoanCollateral>
    {
        public LasoLoanCollateralMapper()
        {
            CsvHelperMapper.MapAll<LoanCollateral>(this);
        }
    }

    public class LasoLoanTransactionMapper : ClassMap<LoanTransaction>
    {
        public LasoLoanTransactionMapper()
        {
            CsvHelperMapper.MapAll<LoanTransaction>(this);
        }
    }

    public class LasoLoanApplicationMapper : ClassMap<LoanApplication>
    {
        public LasoLoanApplicationMapper()
        {
            CsvHelperMapper.MapAll<LoanApplication>(this);
        }
    }

    public class LasoLoanAttributeMapper : ClassMap<LoanAttribute>
    {
        public LasoLoanAttributeMapper()
        {
            CsvHelperMapper.MapAll<LoanAttribute>(this);
        }
    }

    // can't use inheritance from a generic type for these mappers 
    // because CsvHelper creates an instance of each, so use a static helper.
    internal static class CsvHelperMapper
    {      
        public static void MapAll<T>(ClassMap instanceMapper, Dictionary<string, string> overrides = default) where T : ILasoEntity
        {
            var type = typeof(T);          

            foreach (var prop in type.GetProperties())
            {
                if (overrides != null && overrides.TryGetValue(prop.Name, out string mapAs))
                {
                    instanceMapper.Map(type, prop).Name(mapAs);
                }
                else
                {
                    instanceMapper.Map(type, prop).Name(MappingHelper.ToUnderscoreDelimited(prop.Name));
                }
            }
        }
    }
}
