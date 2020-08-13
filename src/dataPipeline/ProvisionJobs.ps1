$jobs = @(
@{ name = "data-router:blue";		service = "data-router" },
@{ name = "data-router:green";		service = "data-router" },
@{ name = "qa-data-router:latest";	service = "data-router" }, 
@{ name = "data-quality:blue";		service = "data-quality" },
@{ name = "data-quality:green";		service = "data-quality" },
@{ name = "qa-data-quality:latest"; service = "data-quality" }
)

$jobs | %{ 
	$name = $_.name
	$service = $_.service
	Write-Host "Creating job $name for $service"
	python databricks\job.py create `
		--jobName $name `
		--library "dbfs:/apps/$service/$service-0.0.0/$service-0.0.0.zip" `
		--entryPoint "dbfs:/apps/$service/$service-0.0.0/__dbs-main__.py"  `
		--initScript "dbfs:/apps/$service/$service-0.0.0/init_scripts/install_requirements.sh"
}


