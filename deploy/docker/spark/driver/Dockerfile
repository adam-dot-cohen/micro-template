FROM laso/spark-base:2.4.5-hadoop3.2.1

LABEL AUTHOR="LASO"

# PYTHON LIBRARIES
RUN apt-get update \
 && pip install jsons \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

COPY driver.sh /
RUN chmod +x /driver.sh

CMD ["/bin/bash", "/driver.sh"]
