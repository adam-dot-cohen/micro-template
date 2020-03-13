Instructions for running tests:

A. Local execution:
1. Turn authentication on in local
    Projects: Project AdminPortal.Web 
    Projetct Identity.Api   
    File name: appsettings.Development.json
        "Authentication": {
                "Enabled": true,
            ..}

2. To Run From Command Line:
...\src\Insights.UITests>dotnet test

B. Execution on develop environment: 
...\src\Insights.UITests>dotnet test -s develop.runsettings

develop.runsettings passes the parameters of the develop environment to the runner.