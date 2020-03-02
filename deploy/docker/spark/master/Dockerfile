FROM laso/spark-base:2.4.5-hadoop3.2.1

LABEL AUTHOR="LASO"


COPY master.sh /
RUN chmod +x /master.sh

ENV SPARK_MASTER_PORT 7077
ENV SPARK_MASTER_WEBUI_PORT 8080
ENV SPARK_MASTER_LOG $SPARK_HOME/logs

EXPOSE 8080 7077 6066

CMD ["/bin/bash", "/master.sh"]
