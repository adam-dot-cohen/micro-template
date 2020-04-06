import logging
import azure.functions as func

import requests
import base64
import argparse

#ToDo tokens 
DOMAIN = 'eastus.azuredatabricks.net'
TOKEN = 'dapi0a236544cd2b94e8f3309577b28e4294'

def run_job(jobId, payload):

  #ToDo: pass in job_id

  response = requests.post(
                'https://%s/api/2.0/jobs/run-now' % (DOMAIN),
                headers={'Authorization': 'Bearer %s' % TOKEN},
                json =
                    {
                    "job_id": f"{jobId}",
                    "python_params": [
                                f"{payload}"
                                ]    
                    }
                )

  return response.json()


def main(message: func.ServiceBusMessage):
    # Log the Service Bus Message as plaintext

    message_content_type = message.content_type
    message_body = message.get_body().decode("utf-8")

    logging.info("Python ServiceBus topic trigger processed message.")
    #logging.info("Message Content Type: " + message_content_type)
    logging.info("Message Body: " + message_body)

    #ToDo: consider passing in job name as opposed to jobId.
    resp = run_job(2,message_body)

    logging.info(f"DBR job api response: {resp}")

