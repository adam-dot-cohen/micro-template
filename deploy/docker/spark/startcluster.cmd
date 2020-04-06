@echo off

if [%1] == [] (
	SET WORKERS=1
) ELSE (
	SET WORKERS=%1
)

echo Starting Spark Cluster with %WORKERS% workers
docker-compose --compatibility up --detach --scale spark-worker=%WORKERS%
