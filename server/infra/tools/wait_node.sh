#!/bin/bash

ATTEMPT=0
while true ; do
	curl --data '{"method":"web3_clientVersion","params":[],"id":1,"jsonrpc":"2.0"}' \
		-H "Content-Type: application/json" \
		-X POST $1:30545 1>/dev/null 2>/dev/null
	if [ $? -eq 0 ]; then
		echo "ok"
		break
	fi
	if [ $ATTEMPT -eq 0 ]; then
		printf "waiting for node $1 available"
	fi
	ATTEMPT=$((ATTEMPT+1))
	printf "."
done
