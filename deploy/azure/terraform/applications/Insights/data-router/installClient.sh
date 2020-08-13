#!/bin/bash
set -euo pipefail

sudo apt-get install -y python3-setuptools
sudo apt-get install jq
pip3 install wheel
pip3 install databricks-cli
sudo ln -s /home/vsts/.local/bin/databricks /usr/local/bin/databricks