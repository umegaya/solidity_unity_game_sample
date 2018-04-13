#!/bin/bash

if [ -e "$1/volume/config/chain1.json" ] ; then 
	echo "\`cat /hostetc/machine-id\`.toml" 
else 
	echo "path0.toml" 
fi
