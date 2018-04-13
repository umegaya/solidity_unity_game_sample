#!/bin/bash

if [ -e "$1/volume/config/${K8S_PLATFORM}.json" ] ; then 
	echo "\`cat /hostetc/machine-id\`.toml" 
else 
	echo "path0.toml" 
fi
