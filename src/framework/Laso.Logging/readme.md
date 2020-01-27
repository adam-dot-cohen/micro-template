Thus far, initial commit.


dotnet new sln 
dotnet new classlib -lang C# -o Laso.Logging
dotnet new xunit -lang C# -o Laso.Logging.UnitTests
dotnet sln add Laso.Logging/Laso.Logging.csproj
dotnet sln add Laso.Logging.UnitTests/Laso.Logging.UnitTests.csproj

Then lifted ILogService from Core for to get the package building in a pipeline.
