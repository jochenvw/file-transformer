#!/bin/bash

# Executes deployment script in /infra folder
# Reason for doing it like this is so that 
# - I can execute this file in my CD agent as 'regular' bash script
# - I keep the benefits of code completion etc. from VS Code for the .azcli file format

bash -c "../infra/deploy-infra.azcli"