﻿using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Laso.DataImport.Core.Configuration;
using Laso.DataImport.Domain.Quarterspot.Enumerations;
using Laso.DataImport.Domain.Quarterspot.Models;
using Microsoft.Extensions.Options;

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

        private async Task<IEnumerable<T>> Query<T>(string sql)
		{
			using var connection = new SqlConnection(_config.QsRepositoryConnectionString);
			connection.Open();

			return await connection.QueryAsync<T>(sql);
		}

        #region Queries

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
