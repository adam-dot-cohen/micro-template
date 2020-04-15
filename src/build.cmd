@echo off
powershell -Command "& { . .\solutionscripts\build.ps1; Build-Insights }"
