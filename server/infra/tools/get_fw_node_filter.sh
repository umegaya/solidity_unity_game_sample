#!/bin/bash

ROOT=$(cd $(dirname $0) && pwd)/..
source ${ROOT}/tools/common.sh ${ROOT}

case "$K8S_PLATFORM" in
# gcp: $1 namespace name
"gcp") gcpd compute instances list --format json | jq -r ".[].tags|.items[0]" | sort -u | grep gke-$1;;
*) echo "${K8S_PLATFORM}: no match!!" && exit 1;;
esac
