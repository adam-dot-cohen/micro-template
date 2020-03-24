import requests
import base64
import argparse

DOMAIN = 'eastus.azuredatabricks.net'
TOKEN = 'dapiee0e69c728e5471e3d280398ec0e3769'



def create_job():

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
    response = requests.post(
        'https://%s/api/2.0/jobs/create' % (DOMAIN),
        headers={'Authorization': 'Bearer %s' % TOKEN},
        json =
            {
            "name": "testDeployApp_pyTask",
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
                    {"dbfs": {"destination": "dbfs:/apps/testDeployApp/init_scripts/install_requirements.sh"} }
                ] 
            },
            "libraries": [
              {
                "jar": "dbfs:/apps/testDeployApp/testDeployApp.zip"
              }
            ],
            "spark_python_task": {
                "python_file": "dbfs:/apps/testDeployApp/__main__.py",
                "parameters": [
                  "--testArg", "testvalue"
                ]
            }
        }
    )
    #Use "existing_cluster_id": "0310-193024-tout512" is allowed with spark_python_task.
    

    return response.json()





def run_job():
  response = requests.post(
  'https://%s/api/2.0/jobs/run-now' % (DOMAIN),
  headers={'Authorization': 'Bearer %s' % TOKEN},
  json =
      {
      "job_id": "25",
      "python_params": [
                  "--testArg", "testValue2"
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

  args = parseArg.parse_args()

  # if args.dbSchema:
  #   print ("arg dbSchema: {pValue}".format(pValue=args.dbSchema))

  #end parameters

  result={}
  if args.jobAction == "create":
    result=create_job()
  if args.jobAction == "run":
    result=run_job()

  print(result)  



if __name__ == "__main__":
	main()