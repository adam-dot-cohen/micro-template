using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Partner.Domain.Quarterspot.Models;
using Partner.Core.Configuration;
using Partner.Domain.Quarterspot.Enumerations;

// todo: needs results paging

namespace Partner.Data.Quarterspot
{
	// this is currently implemented only to export certain bits of data from the legacy 
	// repo into Insights. If this needs to grow to accept other uses cases it would be
	// a good idea to rethink how this works. There's currently no ability to filter DB
	// side, get by ID, etc, and we're just using Dapper. Could also use paging capability.
    public class QuarterspotRepository : IQuarterspotRepository
    {
		private readonly IApplicationConfiguration _config;

		public QuarterspotRepository(IApplicationConfiguration config)
		{
			_config = config;
		}

		public async Task<IEnumerable<QsCustomer>> GetCustomersAsync()
		{
			return await Query<QsCustomer>(CustomersQuery);
		}

		public async Task<IEnumerable<QsBusiness>> GetBusinessesAsync()
        {			
			return await Query<QsBusiness>(BusinessQuery);
		}		

		private async Task<IEnumerable<T>> Query<T>(string sql)
		{
			using var connection = new SqlConnection(_config.QsRepositoryConnectionString);
			connection.Open();

			return await connection.QueryAsync<T>(sql);
		}

        #region Queries

		private static readonly string BusinessQuery = 
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
            ) AS [SIC];";

		private static readonly string CustomersQuery =
			@$"SELECT 
				[Query].[Customer_Id] AS {nameof(QsCustomer.Id)},
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
						CASE WHEN ([Lead_BP].[SsnLast4] IS NULL) 
							THEN 
								N'' 
							ELSE 
								[Lead_BP].[SsnLast4] 
							END 
							+ N'-' + 
							CASE WHEN ([BUS].[TaxId] IS NULL) 
								THEN 
									N'' 
								ELSE 
									[BUS].[TaxId] 
								END 
							AS [Customer_Id], 
						[EM].[Id] AS [Id3],         
						[EM].[DecimalValue] AS [DecimalValue], 
						[EM].[EffectiveTime] AS [EffectiveTime],               
						CASE WHEN ([EM].[Id] IS NULL) 
							THEN 
								CAST(NULL AS int) 
							ELSE 
								1 
							END AS [C2]
					FROM [dbo].[Businesses] AS [BUS]
					INNER JOIN  
						(
							SELECT 
								[L].[Id] AS [Lead_Id], 
								[L].[Created] AS [Lead_Created], 
								[L].[Business_Id] AS [Business_Id], 
								[BP_Info].[Id] AS [BusinessPrincipal_Id], 
								[BP_Info].[SsnLast4] AS [SsnLast4], 
								[BP_Info].[CreditScore] AS [CreditScore]					
							FROM  
								[dbo].[Leads] AS [L]
							INNER JOIN  
								(
									SELECT 
										[BP].[Id] AS [Id], 
										[BP].[SsnLast4] AS [SsnLast4], 
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
				ORDER BY [Query].[Id] ASC, [Query].[Lead_Id] ASC, [Query].[BusinessPrincipal_Id] ASC, [Query].[C2] ASC;";

		#endregion Queries
	}
}
