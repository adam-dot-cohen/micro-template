dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool install dotnet-reportgenerator-globaltool --tool-path tools

dotnet test ./laso.logging.sln --configuration Debug /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./testResults/unitTest.lcov.info 

cd testResults
reportgenerator -reports:unitTest.lcov.info -targetdir:./ -reportTypes:htmlInline
intex.htm
cd..


