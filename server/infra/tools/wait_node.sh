#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

ATTEMPT=0
while true ; do
	jsonrpc '{"method":"web3_clientVersion","params":[],"id":1,"jsonrpc":"2.0"}' $1 1>/dev/null 2>/dev/null
	if [ $? -eq 0 ]; then
		break
	fi
	if [ $ATTEMPT -eq 0 ]; then
		printf "waiting for node $1 available"
	fi
	ATTEMPT=$((ATTEMPT+1))
	printf "."
done
