@echo off
powershell -Command "& { . .\solutionscripts\start.ps1; . .\solutionscripts\test.ps1; Test-Insights %1 %2 %3 %4 %5 }"
