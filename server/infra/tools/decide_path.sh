#!/bin/bash

if [ -e "$1/volume/config/${K8S_PLATFORM}.json" ] ; then 
	if [ -e "$1/volume/config/${K8S_PLATFORM}.toml"  ]; then
		echo "${K8S_PLATFORM}.toml" 
	else
		echo "\`cat /hostetc/machine-id\`.toml" 
	fi
else 
	echo "path0.toml" 
fi
