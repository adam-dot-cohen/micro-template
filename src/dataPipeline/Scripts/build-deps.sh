#! /bin/bash


source .venv/bin/activate
pip install -r data-quality-requirements.txt
pushd .venv/lib/python3.7/site-packages && zip -r /mnt/data/app/dist/data-quality-1.0-deps.zip . && popd 
