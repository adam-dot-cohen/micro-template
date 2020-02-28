using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Domain.Quarterspot.Enumerations;
using Laso.DataImport.Domain.Quarterspot.Models;

namespace Laso.DataImport.Data.Quarterspot
{
	// this is currently implemented only to export certain bits of data from the legacy 
	// repo into Insights. If this needs to grow to accept other uses cases it would be
	// a good idea to rethink how this works. There's currently no ability to filter DB
	// side, get by ID, etc, and we're just using Dapper. Could also use paging capability.
    public class QuarterspotRepository : IQuarterspotRepository
    {
		private readonly IConnectionStringConfiguration _config;

		public QuarterspotRepository(IConnectionStringConfiguration config)
		{
			_config = config;
		}

		public async Task<IEnumerable<QsCustomer>> GetCustomersAsync()
		{
			return await Query<QsCustomer>(CustomersQuery);
        }

		public async Task<IEnumerable<QsCustomer>> GetCustomersAsync(int offset, int take)
		{
			return await Query<QsCustomer>(PagedQuery(CustomersQuery, offset, take));
        }		

		public async Task<IEnumerable<QsBusiness>> GetBusinessesAsync()
        {			
			return await Query<QsBusiness>(BusinessesQuery);
		}

		public async Task<IEnumerable<QsBusiness>> GetBusinessesAsync(int offset, int take)
		{
			return await Query<QsBusiness>(PagedQuery(BusinessesQuery, offset, take));
		}

        public async Task<IEnumerable<QsAccount>> GetAccountsAsync()
        {
			return await Query<QsAccount>(AccountsQuery);
		}

        public async Task<IEnumerable<QsAccount>> GetAccountsAsync(int offset, int take)
        {
			return await Query<QsAccount>(PagedQuery(AccountsQuery, offset, take));
		}

		public async Task<IEnumerable<QsAccountTransaction>> GetAccountTransactionsAsync()
		{
			return await Query<QsAccountTransaction>(AccountTransactionsQuery);
		}

		public async Task<IEnumerable<QsAccountTransaction>> GetAccountTransactionsAsync(int offset, int take)
		{
			return await Query<QsAccountTransaction>(PagedQuery(AccountTransactionsQuery, offset, take));
		}

		public async Task<IEnumerable<QsLoan>> GetLoansAsync()
		{
			return await Query<QsLoan>(LoansQuery);
		}

		public async Task<IEnumerable<QsLoan>> GetLoansAsync(int offset, int take)
		{
			return await Query<QsLoan>(PagedQuery(LoansQuery, offset, take));
		}

        public async Task<IEnumerable<QsLoanMetadata>> GetLoanMetadataAsync()
        {
            return await Query<QsLoanMetadata>(LoanMetadataQuery);
        }

        public async Task<IEnumerable<QsLoanMetadata>> GetLoanMetadataAsync(int offset, int take)
        {
            return await Query<QsLoanMetadata>(PagedQuery(LoanMetadataQuery, offset, take));
        }

		private async Task<IEnumerable<T>> Query<T>(string sql)
		{
            await using var connection = new SqlConnection(_config.QsRepositoryConnectionString);			
			connection.Open();

			return await connection.QueryAsync<T>(sql);
		}

		#region Queries
		
		//! these are all essentially copy and pasted EF generated queries. There is room for improvement if needed...

        private static readonly string LoanMetadataQuery =
            $@"SELECT
	                [L].[Id] AS {nameof(QsLoanMetadata.LeadId)},
	                [B].[Id] AS {nameof(QsLoanMetadata.BusinessId)},
	                [L].[LoanGroupId] AS {nameof(QsLoanMetadata.GroupId)},
	                [P].[Name] AS {nameof(QsLoanMetadata.Product)},
	                [L].[Created] AS {nameof(QsLoanMetadata.ApplicationDate)},
	                [L].[ReportingGroup] AS {nameof(QsLoanMetadata.ReportingGroupValue)},
	                [L].[RequestedLoanAmount] {nameof(QsLoanMetadata.RequestedAmount)},
	                [TOD].[MaxTerm] AS {nameof(QsLoanMetadata.MaxOfferedTerm)},
	                [TD].[InstallmentAmount] AS {nameof(QsLoanMetadata.AcceptedInstallment)},
	                [TD].[InterestRate] AS {nameof(QsLoanMetadata.AcceptedInterestRate)},
	                [TD].[Principal] AS {nameof(QsLoanMetadata.AcceptedAmount)},
	                [T].[Name] AS {nameof(QsLoanMetadata.AcceptedTerm)},
	                [I].[DisplayName] AS {nameof(QsLoanMetadata.AcceptedInstallmentFrequency)},
                    [DR].[Name] AS {nameof(QsLoanMetadata.DeclineReason)}
	                -- declined reason
                FROM [dbo].[Leads] AS L
                LEFT OUTER JOIN [dbo].[Businesses] AS B ON [B].[Id] = [L].[Business_Id]
                INNER JOIN [dbo].[LeadConfigurations] AS LC ON [LC].[Id] = [L].[Configuration_Id]
                LEFT OUTER JOIN [dbo].[ProductChannelOfferings] AS PCO ON [PCO].[Id] = [LC].[ProductChannelOffering_Id]
                LEFT OUTER JOIN [dbo].[Products] AS P ON [P].[Id] = [PCO].[Product_Id]
                LEFT OUTER JOIN [dbo].[TermData] AS TD ON [TD].[Lead_Id] = [L].[Id]
                LEFT OUTER JOIN [dbo].[TermInstallments] AS TI ON [TI].[Id] = [TD].[TermInstallment_Id]
                LEFT OUTER JOIN [dbo].[Terms] AS T ON [T].[Id] = [TI].[Term_Id]
                LEFT OUTER JOIN [dbo].[Installments] AS I ON [I].[Id] = [TI].[Installment_Id]
                LEFT OUTER JOIN [dbo].[TermOfferData] AS TOD ON [TOD].[Lead_Id] = [L].[Id]
                LEFT OUTER JOIN [dbo].[DeclinedReasons] AS DR ON [L].[DeclinedReason_Id] = [DR].[Id]
                ORDER BY [L].[Id]";

		private static readonly string LoansQuery = 
             $@"SELECT 
                    [Project2].[DaysPastDue] AS {nameof(QsLoan.DaysPastDue)}, 
                    [Project2].[Id] AS {nameof(QsLoan.Id)}, 
                    [Project2].[Business_Id] AS {nameof(QsLoan.BusinessId)}, 
                    [Project2].[Name] AS {nameof(QsLoan.ProductType)}, 
                    [Project2].[C1] AS {nameof(QsLoan.IssueDate)}, 
                    [Project2].[CompletedDate] AS {nameof(QsLoan.MaturityDate)}, 
                    [Extent10].[LengthDays] AS {nameof(QsLoan.Term)}, 
                    [Project2].[Payment] AS {nameof(QsLoan.Installment)}, 
                    [Extent11].[DisplayName] AS {nameof(QsLoan.InstallmentFrequency)}
                FROM (SELECT 
                    [Extent1].[Id] AS [Id], 
                    [Extent1].[DaysPastDue] AS [DaysPastDue], 
                    [Extent1].[Payment] AS [Payment], 
                    [Extent1].[CompletedDate] AS [CompletedDate], 
                    [Extent1].[Business_Id] AS [Business_Id], 
                    [Extent1].[Listing_Id] AS [Listing_Id], 
                    [Extent2].[Id] AS [Id1], 
                    [Extent2].[Lead_Id] AS [Lead_Id], 
                    [Extent2].[TermInstallment_Id] AS [TermInstallment_Id], 
                    [Join2].[Id1] AS [Id2], 
                    [Join2].[ProductChannelOffering_Id] AS [ProductChannelOffering_Id], 
                    [Join2].[Id2] AS [Id3], 
                    [Join2].[Configuration_Id] AS [Configuration_Id], 
                    [Extent5].[Id] AS [Id4], 
                    [Extent5].[Product_Id] AS [Product_Id], 
                    [Extent6].[Id] AS [Id5], 
                    [Extent6].[Name] AS [Name], 
                    (SELECT TOP (1) 
                        [Extent8].[ScheduledDate] AS [ScheduledDate]
                        FROM  [dbo].[LoanSchedules] AS [Extent7]
                        INNER JOIN [dbo].[LoanEvents] AS [Extent8] ON [Extent7].[Id] = [Extent8].[Schedule_Id]
                        WHERE ([Extent1].[Id] = [Extent7].[Loan_Id]) AND (100 = [Extent8].[Type])) AS [C1]
                    FROM     [dbo].[Loans] AS [Extent1]
                    INNER JOIN [dbo].[Listings] AS [Extent2] ON [Extent1].[Listing_Id] = [Extent2].[Id]
                    LEFT OUTER JOIN  (SELECT [Extent3].[Id] AS [Id1], [Extent3].[ProductChannelOffering_Id] AS [ProductChannelOffering_Id], [Extent4].[Id] AS [Id2], [Extent4].[Configuration_Id] AS [Configuration_Id]
                        FROM  [dbo].[LeadConfigurations] AS [Extent3]
                        INNER JOIN [dbo].[Leads] AS [Extent4] ON [Extent3].[Id] = [Extent4].[Configuration_Id] ) AS [Join2] ON [Extent2].[Lead_Id] = [Join2].[Id2]
                    LEFT OUTER JOIN [dbo].[ProductChannelOfferings] AS [Extent5] ON [Join2].[ProductChannelOffering_Id] = [Extent5].[Id]
                    LEFT OUTER JOIN [dbo].[Products] AS [Extent6] ON [Extent5].[Product_Id] = [Extent6].[Id] ) AS [Project2]
                LEFT OUTER JOIN [dbo].[TermInstallments] AS [Extent9] ON [Project2].[TermInstallment_Id] = [Extent9].[Id]
                LEFT OUTER JOIN [dbo].[Terms] AS [Extent10] ON [Extent9].[Term_Id] = [Extent10].[Id]
                LEFT OUTER JOIN [dbo].[Installments] AS [Extent11] ON [Extent9].[Installment_Id] = [Extent11].[Id]
                ORDER BY [Project2].[Business_Id]";

		// todo: this is far too naive. Will time out after the second or third page request. 
		// need a faster way to gather this than from Businesses, and paging is a bad idea.
		private static readonly string AccountTransactionsQuery = 
			 $@"SELECT
					[Query].[Category1] AS {nameof(QsAccountTransaction.TransactionCategoryValue)}, 
					[Query].[Id1] AS {nameof(QsAccountTransaction.TransactionId)}, 
					[Query].[Id2] AS {nameof(QsAccountTransaction.AccountId)},
					[Query].[AvailableDate] AS {nameof(QsAccountTransaction.AvailableDate)}, 
					[Query].[PostedDate] AS {nameof(QsAccountTransaction.PostedDate)}, 
					[Query].[Amount] AS {nameof(QsAccountTransaction.Amount)}, 
					[Query].[Memo] AS {nameof(QsAccountTransaction.Memo)}, 
					[Query].[RunningBalance] AS {nameof(QsAccountTransaction.BalanceAfterTransaction)}
				FROM  [dbo].[Businesses] AS [Extent1]
				INNER JOIN  (SELECT [Extent2].[Id] AS [Id3], [Extent2].[User_UserId] AS [User_UserId], [T].[Id2], [T].[BankAccount_Id1], [T].[Id1], [T].[Amount], [T].[RunningBalance], [T].[PostedDate], [T].[AvailableDate], [T].[Memo], [T].[Category1]
					FROM   (SELECT [Var_1].[Id] AS [Id], [Var_1].[User_UserId] AS [User_UserId]
						FROM [dbo].[BankAccounts] AS [Var_1]
						WHERE ([Var_1].[Deleted] = (CAST(NULL AS datetime2))) OR ([Var_1].[Deleted] IS NULL)) AS [Extent2]
					INNER JOIN  (SELECT [Extent3].[Id] AS [Id2], [Extent3].[BankAccount_Id] AS [BankAccount_Id1], [Extent4].[Id] AS [Id1], [Extent4].[Amount] AS [Amount], [Extent4].[RunningBalance] AS [RunningBalance], [Extent4].[PostedDate] AS [PostedDate], [Extent4].[AvailableDate] AS [AvailableDate], [Extent4].[Memo] AS [Memo], [Extent4].[Category] AS [Category1]
						FROM  [dbo].[AggregationBankAccounts] AS [Extent3]
						INNER JOIN [dbo].[BankAccountTransactions] AS [Extent4] ON [Extent3].[Id] = [Extent4].[BankAccount_Id] ) AS [T] ON [Extent2].[Id] = [T].[BankAccount_Id1] ) AS [Query] ON [Query].[User_UserId] = [Extent1].[User_UserId]
				WHERE ((( CAST( [Extent1].[Type] AS int)) & {(int)BusinessType.Borrower}) = {(int)BusinessType.Borrower}) AND ( EXISTS (SELECT 
					1 AS [C1]
					FROM ( SELECT 
						[Extent5].[Id] AS [Id]
						FROM   (SELECT [Var_2].[Id] AS [Id], [Var_2].[User_UserId] AS [User_UserId]
							FROM [dbo].[BankAccounts] AS [Var_2]
							WHERE ([Var_2].[Deleted] = (CAST(NULL AS datetime2))) OR ([Var_2].[Deleted] IS NULL)) AS [Extent5]
						INNER JOIN [dbo].[Businesses] AS [Extent6] ON [Extent5].[User_UserId] = [Extent6].[User_UserId]
						WHERE [Extent1].[Id] = [Extent6].[Id]
					)  AS [Project1]
					WHERE  EXISTS (SELECT 
						1 AS [C1]
						FROM [dbo].[AggregationBankAccounts] AS [Extent7]
						WHERE [Project1].[Id] = [Extent7].[BankAccount_Id]
					)
				))
				ORDER BY [Query].[Id1]";


		private static readonly string AccountsQuery = 
			 $@"SELECT
					 [BA_USER_AGG].[Category] AS {nameof(QsAccount.BankAccountCategoryValue)}
					,[BA_USER_AGG].[AGG_Id] AS {nameof(QsAccount.AccountId)}
					,[BUS].[Id] AS {nameof(QsAccount.BusinessId)}
					,[BA_USER_AGG].[OpenDate] AS {nameof(QsAccount.OpenDate)}
					,[BA_USER_AGG].[Balance] AS {nameof(QsAccount.CurrentBalance)}
					,[BA_USER_AGG].[BalanceDate] AS {nameof(QsAccount.CurrentBalanceDate)}
				FROM  [dbo].[Businesses] AS [BUS]
				INNER JOIN  
				(
					SELECT 
						 [BA_USER_J].[Id] AS [Id2]
						,[BA_USER_J].[User_UserId] AS [User_UserId]
						,[AGG].[Id] AS [AGG_Id]
						,[AGG].[Category] AS [Category]
						,[AGG].[OpenDate] AS [OpenDate]
						,[AGG].[Balance] AS [Balance]
						,[AGG].[BalanceDate] AS [BalanceDate]
					FROM   
					(
						SELECT 
							 [BA].[Id] AS [Id]
							,[BA].[User_UserId] AS [User_UserId]
						FROM 
							[dbo].[BankAccounts] AS [BA]
						WHERE 
							[BA].[Deleted] IS NULL
					) AS [BA_USER_J]
					INNER JOIN 
						[dbo].[AggregationBankAccounts] AS [AGG] ON [BA_USER_J].[Id] = [AGG].[BankAccount_Id] 
				) AS [BA_USER_AGG] ON [BA_USER_AGG].[User_UserId] = [BUS].[User_UserId]
				WHERE 
					(([BUS].[Type] & {(int)BusinessType.Borrower}) = {(int)BusinessType.Borrower})
					AND 
					( 
						EXISTS 
						(
							SELECT
								1 AS [C1]
								FROM 
								( 
									SELECT 
										[BA_USER].[Id] AS [Id]
									FROM   
									(
										SELECT 
											 [Var_2].[Id] AS [Id]
											,[Var_2].[User_UserId] AS [User_UserId]
										FROM 
											[dbo].[BankAccounts] AS [Var_2]
										WHERE 
											[Var_2].[Deleted] IS NULL
									) AS [BA_USER]
									INNER JOIN 
										[dbo].[Businesses] AS [BUS2] ON [BA_USER].[User_UserId] = [BUS2].[User_UserId]
									WHERE 
										[BUS].[Id] = [BUS2].[Id]
								) AS [USER]
							WHERE  EXISTS (SELECT 
								1 AS [C1]
								FROM 
									[dbo].[AggregationBankAccounts] AS [AGG2]
								WHERE 
									[USER].[Id] = [AGG2].[BankAccount_Id]
							)
						)
					)
				ORDER BY [BUS].[Id]";

		private static readonly string BusinessesQuery = 
			$@"SELECT 
	             [B].[Business_Id] AS {nameof(QsBusiness.Id)}
	            ,[B].[Established] AS {nameof(QsBusiness.Established)}
	            ,[B].[Code] AS {nameof(QsBusiness.IndustryNaicsCode)}
	            ,[SIC].[Code] AS {nameof(QsBusiness.IndustrySicCode)}
	            ,[B].[BusinessEntityType] AS {nameof(QsBusiness.BusinessEntityType)}
	            ,[B].[LegalName] AS {nameof(QsBusiness.LegalName)}
	            ,[B].[Phone] AS {nameof(QsBusiness.Phone)}
	            ,[B].[TaxId] AS {nameof(QsBusiness.TaxId)}
	            ,[B].[Zip] AS {nameof(QsBusiness.Zip)}
            FROM 
            (
	            SELECT  
			         [B2].[Id] AS [Business_Id]
		            ,[B2].[Established] AS [Established]
		            ,[I].[Code] AS [Code]
		            ,[B2].[BusinessEntityType] AS [BusinessEntityType]
		            ,[B2].[LegalName] AS [LegalName]
		            ,[B2].[Phone] AS [Phone]
		            ,[B2].[TaxId] AS [TaxId]			  
		            ,[B2].[Zip] AS [Zip]
		            ,[B2].[Industry_Id] AS [Industry_Id]			 
                FROM  
		            [dbo].[Businesses] AS [B2]
                LEFT OUTER JOIN 
		            [dbo].[Industries] AS [I] 
		            ON [B2].[Industry_Id] = [I].[Id]
                WHERE 
		            ([B2].[Type] & {(int)BusinessType.Borrower}) = {(int)BusinessType.Borrower}
            ) AS [B]
            OUTER APPLY  
            (
	            SELECT TOP (1) 
		            [SIC2].[Code] AS [Code]
                FROM  
		            [dbo].[SicCodeIndustries] AS SICI
                INNER JOIN 
		            [dbo].[SicCodes] AS [SIC2] 
		            ON [SICI].[SicCode_Id] = [SIC2].[Id]
                WHERE 
		            [B].[Industry_Id] = [SICI].[Industry_Id] 
            ) AS [SIC]
			ORDER BY [B].[Business_Id]";

		// todo(ed s): need source for credit score (reported or EM source)
		private static readonly string CustomersQuery =
			 @$"SELECT 
			        [Query].[BusinessPrincipal_Id] AS {nameof(QsCustomer.PrincipalId)},
				    CASE WHEN ([Query].[DecimalValue] IS NULL)
					    THEN 
						    [Query].[CreditScore]
					    ELSE
					 	   [Query].[DecimalValue]
				    END AS {nameof(QsCustomer.CreditScore)},
				    CASE WHEN ([Query].[DecimalValue] IS NULL)
					    THEN 
					 	   [Query].[LeadCreated]
					    ELSE
					 	   [Query].[EffectiveTime]
				    END AS {nameof(QsCustomer.CreditScoreEffectiveTime)}
				FROM 
				( 
					SELECT 
						[BUS].[Id] AS [Id], 
						[BUS].[Type] AS [Type], 
						[Lead_BP].[Lead_Id] AS [Lead_Id], 
						[Lead_BP].[Lead_Created] AS [LeadCreated], 
						[Lead_BP].[BusinessPrincipal_Id] AS [BusinessPrincipal_Id], 
						[Lead_BP].[CreditScore] AS [CreditScore], 						
						[EM].[Id] AS [Id3],         
						[EM].[DecimalValue] AS [DecimalValue], 
						[EM].[EffectiveTime] AS [EffectiveTime]
					FROM [dbo].[Businesses] AS [BUS]
					INNER JOIN  
						(
							SELECT 
								[L].[Id] AS [Lead_Id], 
								[L].[Created] AS [Lead_Created], 
								[L].[Business_Id] AS [Business_Id], 
								[BP_Info].[Id] AS [BusinessPrincipal_Id],
								[BP_Info].[CreditScore] AS [CreditScore]
							FROM  
								[dbo].[Leads] AS [L]
							INNER JOIN  
								(
									SELECT 
										[BP].[Id] AS [Id],                                         
										[BP].[CreditScore] AS [CreditScore], 
										[BP].[Lead_Id] AS [Lead_Id]
									FROM 
										[dbo].[BusinessPrincipals] AS [BP]
									WHERE 
										([BP].[Deleted] = (CAST(NULL AS datetime2)))
										OR ([BP].[Deleted] IS NULL) 
								) AS [BP_Info] 
								ON ([L].[Id] = [BP_Info].[Lead_Id]) 
									AND ([BP_Info].[CreditScore] IS NOT NULL) 
						) AS [Lead_BP] 
						ON [BUS].[Id] = [Lead_BP].[Business_Id]
					LEFT OUTER JOIN [dbo].[EvaluationMetrics] AS [EM] 
						ON  [EM].[Discriminator] = N'DecimalEvaluationMetric'
							AND ([Lead_BP].[BusinessPrincipal_Id] = [EM].[Owner_Id]) 
							AND ([EM].[Type_Id] = {(int)EvaluationMetricTypeValue.CreditScore})				
					WHERE 
						([BUS].[Type] & {(int)BusinessType.Borrower}) = {(int)BusinessType.Borrower}
				)  AS [Query]
				ORDER BY [Query].[Id] ASC, [Query].[Lead_Id] ASC, [Query].[BusinessPrincipal_Id] ASC";

		private static string PagedQuery(string query, int offset, int take)
		{
			return @$"{query} OFFSET {offset} ROWS FETCH NEXT {take} ROWS ONLY";
		}		

		#endregion Queries
	}
}
