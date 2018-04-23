#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

PODS=(`kcd get po -o json | jq -r .items[].metadata.name`)

for p in ${PODS[@]} ; do
	kcd exec ${p} kill 1
done
