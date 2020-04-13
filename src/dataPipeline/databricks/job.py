# DEBUG ARGS: --jobAction create --jobName data-router.0.1.0 --library "dbfs:/apps/data-router/data-router-0.1.0/data-router-0.1.0.zip" --entryPoint "dbfs:/apps/data-router/data-router-0.1.0/__dbs-main__.py"  --initScript "dbfs:/apps/data-router/data-router-0.1.0/init_scripts/install_requirements.sh"
import requests
import base64
import argparse
import json
import pprint

DOMAIN = 'eastus.azuredatabricks.net'
TOKEN = 'dapi0a236544cd2b94e8f3309577b28e4294'

class Dictate(object):
    """Object view of a dict, updating the passed in dict when values are set
    or deleted. "Dictate" the contents of a dict...: """

    def __init__(self, d):
        # since __setattr__ is overridden, self.__dict = d doesn't work
        object.__setattr__(self, '_Dictate__dict', d)

    # Dictionary-like access / updates
    def __getitem__(self, name):
        value = self.__dict[name]
        if isinstance(value, dict):  # recursively view sub-dicts as objects
            value = Dictate(value)
        elif isinstance(value, list):
            value = [Dictate(x) for x in value]
        return value

    def __setitem__(self, name, value):
        self.__dict[name] = value
    def __delitem__(self, name):
        del self.__dict[name]

    # Object-like access / updates
    def __getattr__(self, name):
        return self[name]

    def __setattr__(self, name, value):
        self[name] = value
    def __delattr__(self, name):
        del self[name]

    def __repr__(self):
        return "%s(%r)" % (type(self).__name__, self.__dict)
    def __str__(self):
        return str(self.__dict)

# ToDo: allow parameters to pass multiple init_scripts and libraries.   


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

def create_job(jobName, initScript, library, entryPoint, num_workers: int = 3):
    # init_scripts -> have to be local to DBR. Rest of files can live in mount drives if choose to.
    response = requests.post(
        f'https://{DOMAIN}/api/2.0/jobs/create',
        headers={'Authorization': f'Bearer {TOKEN}'},
        json =
            {
            "name": f"{jobName}",
            "new_cluster": {
                "spark_version": "6.3.x-scala2.11",
                "node_type_id": "Standard_DS3_v2",
                "num_workers": num_workers,
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
                "parameters": [ ]
            }
        }
    )

    #Use "existing_cluster_id": "0310-193024-tout512" is allowed with spark_python_task.

    return response.json()


def get_job_id(job_name: str) -> int:
  return get_jobs().get(job_name, -1)
  
def get_jobs(map: bool = True):
  response = requests.get(f'https://{DOMAIN}/api/2.0/jobs/list', headers={'Authorization': f'Bearer {TOKEN}'} )
  result = response.json()
  obj = Dictate(result)
  if map:
    return {job.settings.name:job.job_id for job in obj.jobs}
  else:
    return obj.jobs

def get_job(job_id: int = None, job_name: str=None):
  jobs = get_jobs(False)
  job = None
  if not job_id is None:
      job, = (j for j in get_jobs(False) if j.job_id == job_id)
  elif not job_name is None:
      job, = (j for j in get_jobs(False) if j.settings.name == job_name)

  return job

def update_job(job_name: str, library: str, is_test: bool = False ):
  job = get_job(job_name)
  job.settings.libraries.clear()

  if not library.startswith('dbfs:/'):
    appName = 'test' if is_test else library[:library.rfind('.')]
    library = f'dbfs:/apps/{appName}/{library}'

  job.settings.libraries.append( {'jar': library} )
  response = requests.post(f'https://{DOMAIN}/api/2.0/jobs/reset', headers={'Authorization': f'Bearer {TOKEN}'}, json = list(job.settings.__dict__.values())[0])
  return response.json()

def run_job(job_name: str = None, job_id: int = None, params: str=None, param_file: str=None):
  if not (param_file is None):
    with open(param_file, 'r') as file:
      params = file.read()

  if not (job_name is None):
    job_id = get_job_id(job_name)

  response = requests.post(
    f'https://{DOMAIN}/api/2.0/jobs/run-now',
    headers={'Authorization': f'Bearer {TOKEN}'},
    json =
        {
          "job_id": job_id,
          "python_params": [ params ]
                      #"{\"CorrelationId\": \"0B9848C2-5DB5-43AE-B641-87272AF3ABDD\",\"PartnerId\": \"93383d2d-07fd-488f-938b-f9ce1960fee3\",\"PartnerName\": \"Demo Partner\",\"Files\": [{\"Id\": \"4ebd333a-dc20-432e-8888-bf6b94ba6000\",\"Uri\": \"https://lasodevinsightsescrow.blob.core.windows.net/93383d2d-07fd-488f-938b-f9ce1960fee3/SterlingNational_Laso_R_AccountTransaction_11107019_11107019095900.csv\",\"ContentLength\": 89885,\"ETag\": \"0x8D7CB7471BA8000\",\"DataCategory\": \"AccountTransaction\"}]}"
                  # ]    
        }
  )

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
            #//"console": "integratedTerminal",
            #// "args": [
            #//     "--jobAction", "create", 
            #//     "--jobName", "test-data-quality", 
            #//     "--library", "dbfs:/apps/data-quality/data-quality-0.1.0/data-quality-0.1.0.zip", 
            #//     "--entryPoint", "dbfs:/apps/data-quality/data-quality-0.1.0/__dbs-main__.py",  
            #//     "--initScript", "dbfs:/apps/data-quality/data-quality-0.1.0/init_scripts/install_requirements.sh"
            #// ]
#DEBUG ARG: --jobAction create --jobName data-router.0.1.0 --library "dbfs:/apps/data-router/data-router-0.1.0/data-router-0.1.0.zip" --entryPoint "dbfs:/apps/data-router/data-router-0.1.0/__dbs-main__.py"  --initScript "dbfs:/apps/data-router/data-router-0.1.0/init_scripts/install_requirements.sh"

def main():
  """
  Parse parameters.
  * = optional

  jobAction: submit, create, run, list, getid, get, update
  jobName
  """
  #logger = logging.getLogger("__name__") #name of the module

  parseArg = argparse.ArgumentParser()
  parseArg.add_argument("jobAction", type=str, choices=['create','run','list','getid','get','update'], help="job action to take")
  parseArg.add_argument("-i","--jobId", type=int, help="id of the job")
  parseArg.add_argument("-n", "--jobName", type=str, help="name of the job")
  parseArg.add_argument("-p", "--paramsFile", help="name of the file with json payload, used with 'run' action")
  parseArg.add_argument("-s", "--initScript", help="path of the job init script, using dbfs:/ notation")
  parseArg.add_argument("-l", "--library", help="path of the job application library (zip file), using dbfs:/ notation")
  parseArg.add_argument("-e", "--entryPoint", help="path of the py file containing the main entrypoint for the job, using dbfs:/ notation")

  args = parseArg.parse_args()

  # if args.dbSchema:
  #   print ("arg dbSchema: {pValue}".format(pValue=args.dbSchema))

  #end parameters

  result={}
  if args.jobAction == "create":
    result=create_job(args.jobName, args.initScript, args.library, args.entryPoint)
  elif args.jobAction == "run":
    result=run_job(args.jobName, args.jobId, param_file=args.paramsFile)
  elif args.jobAction == 'list':
    result = get_jobs()
  elif args.jobAction == 'getid':
    if args.jobId is None: 
        parseArg.print_help()
        return
    job = get_job(job_id = args.jobId)
    result = list(job.__dict__.values())[0] # needed because of our wrapper
  elif args.jobAction == 'get':
    if args.jobName is None: 
        parseArg.print_help()
        return
    job = get_job(job_name = args.jobName)
    result = list(job.__dict__.values())[0] # needed because of our wrapper
  elif args.jobAction == 'update':
    result = update_job(args.jobName, args.library, True)

  pprint.pprint(result)  


if __name__ == "__main__":
	main()
