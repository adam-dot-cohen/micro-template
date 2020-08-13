<Query Kind="Program">
  <Reference Relative="..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\EntityFramework.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\EntityFramework.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\EntityFramework.SqlServer.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\EntityFramework.SqlServer.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\Infrastructure.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\Infrastructure.dll</Reference>
  <Reference Relative="..\..\..\..\..\..\..\Platform\src\QS.Admin\bin\QS.Core.dll">E:\Work\Repos\Platform\src\QS.Admin\bin\QS.Core.dll</Reference>
  <Namespace>System.Threading.Tasks</Namespace>
  <CopyLocal>true</CopyLocal>
</Query>

async Task Main()
{
	var filePath = @"E:\Work\Insights\Partners\SNB\Data\Decrypted";
	var inputFileName = $@"{filePath}\SterlingNational_Laso_R_AccountTransaction_11072019_01012016.csv";
	var outputFileName = Path.ChangeExtension(inputFileName, ".escaped.csv");
	
	using (var writer = new StreamWriter(outputFileName))
	{
		var inQuote = false;
		using (var reader = File.OpenText(inputFileName))
		{
			var outputLineBuilder = new StringBuilder();
			
			string line;
			while ((line = await reader.ReadLineAsync()) != null)
			{
				for (var i = 0; i < line.Length; i++)
				{
					var character = line[i];
					
					//$"inQuote = {inQuote}, character = {character}".Dump();

					if (character == '"')
					{
						var nextCharIndex = i + 1;
						if (inQuote && (nextCharIndex < line.Length))
						{
							inQuote = line[nextCharIndex] != ',';
							
							if (inQuote)
								outputLineBuilder.Append('"');
						}
						else
							inQuote = true;
					}
					
					outputLineBuilder.Append(character);
				}
								
				var outputLine = outputLineBuilder.ToString();
				await writer.WriteLineAsync(outputLine);
				outputLineBuilder.Clear();
				
				inQuote = false;
			}
		}
	}
}