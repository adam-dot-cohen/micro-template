
#check if notebookk exists
#if !esixts
databricks runs submit --json-file "$($env:WORKING_DIRECTORY)/runNotebook.json"


#check if cluster exists
#do the thing....