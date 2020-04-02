import requests
import base64
import argparse

DOMAIN = 'eastus.azuredatabricks.net'
TOKEN = 'dapi0a236544cd2b94e8f3309577b28e4294'


# ToDo: allow parameters to pass multiple init_scripts and libraries.   
def create_job(jobName, initScript, library, entryPoint):

    # #create job w/new_cluster. Best practice per databricks.    
    # response = requests.post(
    #     'https://%s/api/2.0/jobs/create' % (DOMAIN),
    #     headers={'Authorization': 'Bearer %s' % TOKEN},
    #     json =
    #         {
    #         "name": "testDeployApp_param",
    #         "new_cluster": {
    #             "spark_version": "6.3.x-scala2.11",
    #             "node_type_id": "Standard_DS3_v2",
    #             "num_workers": 3,
    #             "cluster_log_conf": {
    #                 "dbfs" : {
    #                   "destination": "dbfs:/cluster_logs"
    #                 }
    #               },
    #             "init_scripts": [ 
    #                 {"dbfs": {"destination": "dbfs:/apps/testDeployApp/init_scripts/install_requirements.sh"} }
    #             ] 
    #         },
    #         "spark_submit_task": {
    #             "parameters": [
    #               "--py-files",
    #               "dbfs:/apps/testDeployApp/testDeployApp.zip",
    #               "dbfs:/apps/testDeployApp/__main__.py"
    #               ]
    #         }
    #         }
    # )
    


    #run a python file.    

    # init_scripts -> have to be local to DBR. Rest of files can live in mount drives if choose to.
    response = requests.post(
        'https://%s/api/2.0/jobs/create' % (DOMAIN),
        headers={'Authorization': 'Bearer %s' % TOKEN},
        json =
            {
            "name": f"{jobName}",
            "new_cluster": {
                "spark_version": "6.3.x-scala2.11",
                "node_type_id": "Standard_DS3_v2",
                "num_workers": 3,
                "cluster_log_conf": {
                    "dbfs" : {
                      "destination": "dbfs:/cluster_logs"
                    }
                  },
                "init_scripts": [ 
                    {"dbfs": {"destination": f"{initScript}"} }
                ] 
            },
            "libraries": [
              {
                "jar": f"{library}"
              }
            ],
            "spark_python_task": {
                "python_file": f"{entryPoint}",
                "parameters": [
                  "--testArg", "testvalue"
                ]
            }
        }
    )

    #Use "existing_cluster_id": "0310-193024-tout512" is allowed with spark_python_task.

    return response.json()





def run_job():

  #ToDo: pass in job_id

  response = requests.post(
  'https://%s/api/2.0/jobs/run-now' % (DOMAIN),
  headers={'Authorization': 'Bearer %s' % TOKEN},
  json =
      {
      "job_id": "2",
      "python_params": [
                  "{\"CorrelationId\": \"0B9848C2-5DB5-43AE-B641-87272AF3ABDD\",\"PartnerId\": \"93383d2d-07fd-488f-938b-f9ce1960fee3\",\"PartnerName\": \"Demo Partner\",\"Files\": [{\"Id\": \"4ebd333a-dc20-432e-8888-bf6b94ba6000\",\"Uri\": \"https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv\",\"ContentLength\": 89885,\"ETag\": \"0x8D7CB7471BA8000\",\"DataCategory\": \"AccountTransaction\"}]}"
                ]    
      }
  )
#"spark_submit_params": ["--arg1", "param1"]
  return response.json()
  



''' #create cluster
response = requests.post(
  'https://%s/api/2.0/clusters/create' % (DOMAIN),
  headers={'Authorization': 'Bearer %s' % TOKEN},
  json={
    "cluster_name": "my-cluster",
    "spark_version": "5.5.x-scala2.11",
    "node_type_id": "i3.xlarge",
    "spark_env_vars": {
      "PYSPARK_PYTHON": "/databricks/python3/bin/python3"
    },
    "num_workers": 25
  }
)

if response.status_code == 200:
  print(response.json()['cluster_id'])
else:
  print("Error launching cluster: %s: %s" % (response.json()["error_code"], response.json()["message"]))

'''  


#DEBUG ARG: --jobAction create --jobName data-quality.0.1.0 --library "dbfs:/apps/data-quality/data-quality-0.1.0/data-quality-0.1.0.zip" --entryPoint "dbfs:/apps/data-quality/data-quality-0.1.0/__dbs-main__.py"  --initScript "dbfs:/apps/data-quality/data-quality-0.1.0/init_scripts/install_requirements.sh"
#DEBUG ARG: --jobAction create --jobName data-router.0.1.0 --library "dbfs:/apps/data-router/data-router-0.1.0/data-router-0.1.0.zip" --entryPoint "dbfs:/apps/data-router/data-router-0.1.0/__dbs-main__.py"  --initScript "dbfs:/apps/data-router/data-router-0.1.0/init_scripts/install_requirements.sh"

def main():
  """
  Parse parameters.
  * = optional

  jobAction: submit, create
  jobName
  """
  #logger = logging.getLogger("__name__") #name of the module

  parseArg = argparse.ArgumentParser()
  parseArg.add_argument("--jobAction")
  parseArg.add_argument("--jobId")
  parseArg.add_argument("--jobName")
  parseArg.add_argument("--initScript")
  parseArg.add_argument("--library")
  parseArg.add_argument("--entryPoint")

  args = parseArg.parse_args()

  # if args.dbSchema:
  #   print ("arg dbSchema: {pValue}".format(pValue=args.dbSchema))

  #end parameters

  result={}
  if args.jobAction == "create":
    result=create_job(args.jobName, args.initScript, args.library, args.entryPoint)
  if args.jobAction == "run":
    result=run_job()

  print(result)  



if __name__ == "__main__":
	main()
