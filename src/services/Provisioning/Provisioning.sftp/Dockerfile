FROM ubuntu:18.04

# fix locales
RUN apt-get update \
    && apt-get install -y --no-install-recommends locales \
    && rm -rf /var/lib/apt/lists/* \
    && localedef -i en_US -c -f UTF-8 -A /usr/share/locale/locale.alias en_US.UTF-8

ENV LANG en_US.utf8

# install blobfuse and sftp
RUN apt-get update \
    && apt-get install -y wget \
    apt-utils \
    && wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get remove -y wget \
    && apt-get update \
    && apt-get install -y --no-install-recommends fuse \
    blobfuse \
    libcurl3-gnutls \
    libgnutls30 \
    && apt-get -y install openssh-server \
    vim \
    cron \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /var/run/sshd \
    && rm -f /etc/ssh/ssh_host_*key*

COPY vm/install/ssh/sshd_config /etc/ssh/sshd_config
COPY vm/install/bin/* /usr/local/bin/
COPY vm/install/cron* /etc/cron.d/
COPY vm/configure-new-vm /

RUN chmod +x /usr/local/bin/*
RUN chmod +x /configure-new-vm

EXPOSE 22

ENTRYPOINT ["/configure-new-vm"]
