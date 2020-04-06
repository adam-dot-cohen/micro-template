@echo off

echo Submitting test file to cluster
set ip_address_string="IPv4 Address"
for /f "usebackq tokens=2 delims=:" %%f in (`ipconfig ^| findstr /c:%ip_address_string%`) do echo Your IP Address is: %%f

set "TAB=	"
set TARGET_FILE=%CD%\%1
pushd %SPARK_HOME%
echo %TAB%Submitting %TARGET_FILE%
REM bin\spark-submit --master spark://localhost:7077 --conf spark.driver.host=127.0.0.1 %TARGET_FILE%
bin\spark-submit --master spark://localhost:7077 --conf spark.driver.host=127.0.0.1 %TARGET_FILE%
popd

