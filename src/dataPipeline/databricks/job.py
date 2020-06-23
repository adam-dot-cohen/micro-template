# DEBUG ARGS: --jobAction create --jobName data-router.0.1.0 --library "dbfs:/apps/data-router/data-router-0.1.0/data-router-0.1.0.zip" --entryPoint "dbfs:/apps/data-router/data-router-0.1.0/__dbs-main__.py"  --initScript "dbfs:/apps/data-router/data-router-0.1.0/init_scripts/install_requirements.sh"
# update --jobName test-data-quality --library "dbfs:/apps/data-quality/data-quality-0.1.5/data-quality-0.1.5.zip" --entryPoint "dbfs:/apps/data-quality/data-quality-0.1.5/__dbs-main__.py"  --initScript "dbfs:/apps/data-quality/data-quality-0.1.5/init_scripts/install_requirements.sh"
# update --jobName test-data-quality --library "dbfs:/apps/data-quality/data-quality-0.1.6/data-quality-0.1.6.zip" --entryPoint "dbfs:/apps/data-quality/data-quality-0.1.6/__dbs-main__.py"  --initScript "dbfs:/apps/data-quality/data-quality-0.1.6/init_scripts/install_requirements.sh"
# update --jobName "qa-data-router:latest"  --library "dbfs:/apps/data-router/data-router-0.1.8/data-router-0.1.8.zip" --entryPoint "dbfs:/apps/data-router/data-router-0.1.8/__dbs-main__.py"  --initScript "dbfs:/apps/data-router/data-router-0.1.8/init_scripts/install_requirements.sh"
# run -n test-data-quality -p data-quality\dq-command.msg

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

def get_clusters(map: bool = True):
  response = requests.get(f'https://{DOMAIN}/api/2.0/clusters/list', headers={'Authorization': f'Bearer {TOKEN}'} )
  result = response.json()
  obj = Dictate(result)
  if map:
    return {cluster.settings.cluster_name:cluster.cluster_id for cluster in obj.clusters}
  else:
    return obj.clusters

def get_cluster(cluster_id: int = None, cluster_name: str=None):
  clusters = get_clusters(False)
  cluster = None
  if not cluster_id is None:
      cluster, = (c for c in get_clusters(False) if c.cluster_id == cluster_id)
  elif not cluster_name is None:
      cluster, = (c for c in get_clusters(False) if c.cluster_name == cluster_name)

  return cluster


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
                    "dbfs" : { "destination": "dbfs:/cluster_logs" }
                  },
                "init_scripts": [ {"dbfs": {"destination": f"{initScript}"} } ] 
            },
            "libraries": [ { "jar": f"{library}" } ],
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

def update_job(job_name: str, initScript: str, library: str, entryPoint: str, **kwargs):
    """
    Update an existing job definition.  Arguments must be fully qualified.
    """
    num_workers = kwargs.get('num_workers', 3)
    containerEntryPoint = kwargs.get('containerEntryPoint', None)
    new_job_name = kwargs.get('new_job_name', None)

    job = get_job(job_name=job_name)

    if not new_job_name is None:
        job_name = new_job_name

    if 'existing_cluster_id' in list(job.settings.__dict__.values())[0]:
        update_payload = {
                  "job_id": job.job_id,
                  "new_settings": {
                    "name": f"{job_name}",
                    "existing_cluster_id": job.settings.existing_cluster_id,
                    "libraries": [ { "jar": f"{library}" } ],
                    "timeout_seconds": job.settings.timeout_seconds,
                    "spark_python_task": {
                        "python_file": f"{entryPoint}",
                        "parameters": [ ]
                    }
                  }
                }
        
    else:
        if containerEntryPoint is None:
            update_payload = {
              "job_id": job.job_id,
              "new_settings": {
                "name": f"{job_name}",
                "max_concurrent_runs": job.settings.max_concurrent_runs,
                "new_cluster": list(job.settings.new_cluster.__dict__.values())[0], # use whatever was already defined, patch the init script below
                "libraries": [ { "jar": f"{library}" } ],
                "timeout_seconds": job.settings.timeout_seconds,
                "spark_python_task": {
                    "python_file": f"{entryPoint}",
                    "parameters": [ ]
                }
              }
            }
            update_payload['new_settings']['new_cluster']['num_workers'] = num_workers
            update_payload['new_settings']['new_cluster']['init_scripts'] = [{'dbfs': {'destination': f'{initScript}'}}]
        else:
            update_payload = {
              "job_id": job.job_id,
              "new_settings": {
                "name": f"{job_name}",
                "max_concurrent_runs": job.settings.max_concurrent_runs,
                "new_cluster": list(job.settings.new_cluster.__dict__.values())[0], # use whatever was already defined for the container and scaling
                "timeout_seconds": job.settings.timeout_seconds,
                "spark_python_task": {
                    "python_file": f"{containerEntryPoint}",
                    "parameters": [ ]
                }
              }
            }


    #update_json = {
    #        "name": f"{job_name}",
    #        "new_cluster": {
    #            "spark_version": "6.3.x-scala2.11",
    #            "node_type_id": "Standard_DS3_v2",
    #            "num_workers": num_workers,
    #            "cluster_log_conf": {
    #                "dbfs" : {
    #                  "destination": "dbfs:/cluster_logs"
    #                }
    #              },
    #            "init_scripts": [ 
    #                {"dbfs": {"destination": f"{initScript}"} }
    #            ] 
    #        },
    #        "libraries": [
    #          {
    #            "jar": f"{library}"
    #          }
    #        ],
    #        "spark_python_task": {
    #            "python_file": f"{entryPoint}",
    #            "parameters": [ ]
    #        }
    #    }
    response = requests.post(f'https://{DOMAIN}/api/2.0/jobs/reset', headers={'Authorization': f'Bearer {TOKEN}'}, json = update_payload)
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
  parseArg.add_argument("-nn", "--newJobName", type=str, help="new name of the job")
  parseArg.add_argument("-p", "--paramsFile", help="name of the file with json payload, used with 'run' action")
  parseArg.add_argument("-s", "--initScript", help="path of the job init script, using dbfs:/ notation")
  parseArg.add_argument("-l", "--library", help="path of the job application library (zip file), using dbfs:/ notation")
  parseArg.add_argument("-e", "--entryPoint", help="path of the py file containing the main entrypoint for the job, using dbfs:/ notation")
  parseArg.add_argument("-c", "--container", help="path of the py file containing the main entrypoint for the job, using local posix notation")
  parseArg.add_argument("-cid", "--clusterId", help="id of the cluster")
  parseArg.add_argument("-cname", "--clusterName", help="name of the cluster")

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
    if not args.jobId is None:
        job = get_job(job_id = args.jobId)
        result = list(job.__dict__.values())[0] # needed because of our wrapper
    else:
        if args.clusterId is None:
            parseArg.print_help()
            return
        cluster = get_cluster(cluster_id = args.clusterId)
        result = list(cluster.__dict__.values())[0] # needed because of our wrapper

  elif args.jobAction == 'get':
    if not args.jobName is None:
        job = get_job(job_name = args.jobName)
        result = list(job.__dict__.values())[0] # needed because of our wrapper
    else:
        if args.clusterName is None:
            parseArg.print_help()
            return
        cluster = get_cluster(cluster_name = args.clusterName)
        result = list(cluster.__dict__.values())[0] # needed because of our wrapper

  elif args.jobAction == 'update':
    result = update_job(args.jobName, args.initScript, args.library, args.entryPoint, containerEntryPoint=args.container, new_job_name=args.newJobName)

  pprint.pprint(result)  


if __name__ == "__main__":
	main()
