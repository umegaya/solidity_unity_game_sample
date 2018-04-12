#!/bin/bash

if [ -e "$1/volume/config/path1.toml" ] ; then 
	echo "path1.toml" 
else 
	echo "path0.toml" 
fi
