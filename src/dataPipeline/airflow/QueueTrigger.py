from airflow import DAG
from airflow.operators import BashOperator
from datetime import datetime, timedelta
from azure.servicebus import SubscriptionClient, Message
from airflow.models.baseoperator import BaseOperator
from airflow.utils.decorators import apply_defaults
from airflow.models import Variable


dag = DAG(
    dag_id='DataPipline_Router',
    default_args={
        "owner": "laso",
        'start_date': airflow.utils.dates.days_ago(1),
    },
    schedule_interval='*/1 * * * *',
)
 
class MessageBranchOperator(BaseBranchOperator):
    @apply_defaults
    def __init__(self, name: str, *args, **kwargs) -> None:
        super().__init__(*args, **kwargs)
        self.name = name

      
    def choose_branch(self, context):
        """
        Consume message from subscription, add message to context and return the event type for the route
        """
        message = consume_message(context['connection_string'], context['subscription_name'])
        context['task_instance'].xcom_push(key='message',value=message.body)
        json_params = json.loads(message.body)
        return json_params['EventType']

 
    def consume_message(connection_string, subscription_name):
        # Create the QueueClient
        subscription_client = SubscriptionClient.from_connection_string(connection_string, subscription_name)

        # Receive the message from the queue
        with subscription_client.get_receiver() as subscription_receiver:
            messages = subscription_receiver.fetch_next(max_batch_size=1,timeout=3)
            message = messages[0]
            
                 
 
router = MessageBranchOperator(
    task_id='router',
    dag=dag,
    provide_context=True,
    depends_on_past=True
)
 
 
def trigger_dag_with_context(context, dag_run_obj):
    ti = context['task_instance']
    job_params = ti.xcom_pull(key='job_params', task_ids='router')
    dag_run_obj.payload = {'task_payload': job_params}
    return dag_run_obj

 
accept_op = trigger = TriggerDagRunOperator(
    task_id='accept_payload',
    trigger_dag_id="accept_payload",
    python_callable=trigger_dag_with_context,
    params={'condition_param': True, 'task_payload': '{}'},
    dag=dag,
    provide_context=True,
)
 
ingest_op = DummyOperator(task_id='ingest_payload', dag=dag)

 
router >> accept_op
router >> ingest_op
