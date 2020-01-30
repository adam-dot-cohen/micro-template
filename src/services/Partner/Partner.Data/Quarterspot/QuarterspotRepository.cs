using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Partner.Domain.Quarterspot.Models;

namespace Partner.Data.Quarterspot
{
	// this is currently implemented only to export certain bits of data from the legacy 
	// repo into Insights. If this needs to grow to accept other uses cases it would be
	// a good idea to rethink how this works. There's currently no ability to filter DB
	// side, get by ID, etc, and we're just using Dapper.
    public class QuarterspotRepository : IQuarterspotRepository
    {	
		public Task<IEnumerable<Business>> GetBusinessesAsync()
        {
			var sql = $@"SELECT 
	                         [B].[Business_Id] AS {nameof(Business.Id)}
	                        ,[B].[Established] AS {nameof(Business.Established)}
	                        ,[B].[Code] AS {nameof(Business.IndustryNaicsCode)}
	                        ,[SIC].[Code] AS {nameof(Business.IndustrySicCode)}
	                        ,[B].[BusinessEntityType] AS {nameof(Business.BusinessEntityType)}
	                        ,[B].[LegalName] AS {nameof(Business.LegalName)}
	                        ,[B].[Phone] AS {nameof(Business.Phone)}
	                        ,[B].[TaxId] AS {nameof(Business.TaxId)}
	                        ,[B].[Zip] AS {nameof(Business.Zip)}
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

            using var connection = new SqlConnection("");

			return connection.QueryAsync<Business>(sql);
		}
    }
}
