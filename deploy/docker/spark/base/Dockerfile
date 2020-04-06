# this is using a Debian base
FROM  python:3.7-buster   

LABEL maintainer="LASO"

# LOCALE SETUP
RUN apt-get update \
 && apt-get install -y locales \
 && dpkg-reconfigure -f noninteractive locales \
 && locale-gen C.UTF-8 \
 && /usr/sbin/update-locale LANG=C.UTF-8 \
 && echo "en_US.UTF-8 UTF-8" >> /etc/locale.gen \
 && locale-gen \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

# Users with other locales should set this in their derivative image
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US:en
ENV LC_ALL en_US.UTF-8

# UTILITY SETUP (curl, python3, unzip)
RUN apt-get update \
 && apt-get install -y --no-install-recommends apt-utils \
 && apt-get install -y curl unzip python3-setuptools python3-pip \
# && apt-get install -y curl unzip python3 python3-setuptools python3-pip \
# && ln -s /usr/bin/python3 /usr/bin/python \
 && pip install py4j \
# && easy_install3 pip py4j \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*


# http://blog.stuart.axelbrooke.com/python-3-on-spark-return-of-the-pythonhashseed
ENV PYTHONHASHSEED 0
ENV PYTHONIOENCODING UTF-8
ENV PIP_DISABLE_PIP_VERSION_CHECK 1

# JAVA
RUN wget -qO - https://adoptopenjdk.jfrog.io/adoptopenjdk/api/gpg/key/public | apt-key add - \
 && echo "deb https://adoptopenjdk.jfrog.io/adoptopenjdk/deb/ buster main Release" >> /etc/apt/sources.list

RUN apt-get update -y \
 && apt install adoptopenjdk-8-hotspot -y \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*

#ENV JAVA_HOME /usr/lib/jvm/adoptopenjdk-8-hotspot

# PYTHON LIBRARIES
RUN apt-get update \
 && pip install cerberus pandas pyspark jsons \
 && apt-get clean \
 && rm -rf /var/lib/apt/lists/*


 # ==========END OF PREREQUISITES=================
#"http://archive.apache.org/dist/hadoop/common/hadoop-$HADOOP_VERSION/hadoop-$HADOOP_VERSION.tar.gz" 
  
# HADOOP
ENV HADOOP_VERSION 3.2.1
ENV HADOOP_HOME /usr/hadoop-$HADOOP_VERSION
ENV HADOOP_CONF_DIR=$HADOOP_HOME/etc/hadoop
ENV PATH $PATH:$HADOOP_HOME/bin
RUN curl -sL --retry 3 \
  "https://lasodevstorage.file.core.windows.net/install/Applications/Server/Apache/hadoop-3.2.1.tar.gz?st=2020-03-23T17%3A00%3A00Z&se=2025-03-23T05%3A00%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=OvHJ5OxGJmKgZ%2FMgJ7laawGzGiP8g178gl7xFDtWMAg%3D" \
  | gunzip \
  | tar -x -C /usr/ \
 && rm -rf $HADOOP_HOME/share/doc \
 && chown -R root:root $HADOOP_HOME

# http://www.gtlib.gatech.edu/pub/apache/spark/spark-${SPARK_VERSION}/spark-${SPARK_VERSION}-bin-without-hadoop.tgz
# https://www.apache.org/dyn/closer.lua/spark/spark-2.4.5/spark-2.4.5-bin-without-hadoop.tgz
# https://www.apache.org/dyn/closer.lua/spark/spark-${SPARK_VERSION}/spark-${SPARK_VERSION}-bin-without-hadoop.tgz
# https://archive.apache.org/dist/spark/spark-${SPARK_VERSION}/${SPARK_PACKAGE}.tgz
# SPARK
ENV SPARK_VERSION 2.4.5
ENV SPARK_PACKAGE spark-${SPARK_VERSION}-bin-without-hadoop
ENV SPARK_HOME /usr/spark-${SPARK_VERSION}
ENV PYSPARK_PYTHON /usr/local/bin/python
ENV PYSPARK_DRIVER_PYTHON /usr/local/bin/python
ENV SPARK_DIST_CLASSPATH="$HADOOP_HOME/etc/hadoop/*:$HADOOP_HOME/share/hadoop/common/lib/*:$HADOOP_HOME/share/hadoop/common/*:$HADOOP_HOME/share/hadoop/hdfs/*:$HADOOP_HOME/share/hadoop/hdfs/lib/*:$HADOOP_HOME/share/hadoop/hdfs/*:$HADOOP_HOME/share/hadoop/yarn/lib/*:$HADOOP_HOME/share/hadoop/yarn/*:$HADOOP_HOME/share/hadoop/mapreduce/lib/*:$HADOOP_HOME/share/hadoop/mapreduce/*:$HADOOP_HOME/share/hadoop/tools/lib/*"
ENV PATH $PATH:${SPARK_HOME}/bin
RUN curl -sL --retry 3 \
  "https://lasodevstorage.file.core.windows.net/install/Applications/Server/Apache/spark-2.4.5-bin-without-hadoop.tgz?st=2020-03-23T05%3A00%3A00Z&se=2025-03-23T05%3A00%3A00Z&sp=rl&sv=2018-03-28&sr=f&sig=lRi%2BDmTeVKi9gU5JUggrbMwWLUS8khqhQU3oRopqcz0%3D" \
  | gunzip \
  | tar x -C /usr/ \
 && mv /usr/$SPARK_PACKAGE $SPARK_HOME \
 && chown -R root:root $SPARK_HOME
ENV ENABLE_INIT_DAEMON true
ENV INIT_DAEMON_BASE_URI http://identifier/init-daemon
ENV INIT_DAEMON_STEP spark_master_init

# ==============END OF HADOOP/SPARK======================

COPY wait-for-step.sh /
COPY execute-step.sh /
COPY finish-step.sh /
#Give permission to execute scripts
RUN chmod +x /wait-for-step.sh && chmod +x /execute-step.sh && chmod +x /finish-step.sh

#WORKDIR $SPARK_HOME
#CMD ["bin/spark-class", "org.apache.spark.deploy.master.Master"]
