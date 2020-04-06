#!/bin/bash
VENV=$1; shift
ZIPFILE=$1; shift

pushd /mnt/data/app

# install virtualenv:
pip install virtualenv

# create a new virtualenv:
virtualenv $VENV

# activate the virtualenv:
source $VENV/bin/activate

# install packages in virtual environment:
# pip install numpy
# pip install scipy
# pip install scikit-learn
# pip install pandas


unzip -p $ZIPFILE requirements.txt > requirements.txt
pip install -r requirements.txt


python -m venv .venv
source .venv/bin/activate
pip install -r data-quality-requirements.txt
zip -r dist/data-quality-1.0-deps.zip ./deps