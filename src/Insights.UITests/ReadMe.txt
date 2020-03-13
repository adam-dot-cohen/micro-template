UI Automation

Browser and Driver Information:

As of now, the browser used for testing is chrome. 
More browsers and versions will be added in the near future.

Chromedriver:
Please delete all chromedriver executables that you may have in your development environment.
Right now, for windows systems, the code is downloading the necessary chromedriver version based 
on the chrome version installed in the system.
In the future we will have a solution for installing the browser and corresponding driver programmatically.

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