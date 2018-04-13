#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

CONTEXT=`kcd config current-context | tr -d '\r' | tr -d '\n'`
kcd config view -o json | jq -r .clusters[] | jq -r "select(.name == \"$CONTEXT\").cluster.server" | sed -e 's|https://\([^:]*\).*$|\1|'
