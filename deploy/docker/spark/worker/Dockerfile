FROM laso/spark-base:2.4.5-hadoop3.2.1

LABEL AUTHOR="LASO"


COPY worker.sh /
RUN chmod +x /worker.sh

ENV SPARK_WORKER_WEBUI_PORT 8081
ENV SPARK_WORKER_LOG $SPARK_HOME/logs
ENV SPARK_MASTER "spark://spark-master:7077"

EXPOSE 8081

CMD ["/bin/bash", "/worker.sh"]
