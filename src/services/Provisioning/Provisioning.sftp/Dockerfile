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
	&& mkdir -p /etc/fuse.d \
    && rm -f /etc/ssh/ssh_host_*key*

COPY config/sshd_config /etc/ssh/sshd_config
COPY shell/create-sftp-user /usr/local/bin/
COPY shell/create-partner-sftp-handler /usr/local/bin/
COPY shell/entrypoint /
COPY shell/cron-entries.txt /
COPY azure/mount.sh /usr/local/bin/
COPY azure/provision-mount.sh /etc/fuse.d/

RUN chmod +x /usr/local/bin/create-sftp-user
RUN chmod +x /usr/local/bin/create-partner-sftp-handler
RUN chmod +x /entrypoint
RUN chmod +x /usr/local/bin/mount.sh
RUN chmod +x /etc/fuse.d/provision-mount.sh

EXPOSE 22

ENTRYPOINT ["/entrypoint"]
