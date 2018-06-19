#!/bin/bash
set -e

ROOT=$(cd $(dirname $0) && pwd)/../..
FNROOT=${ROOT}/dist

gcp() {
	gcloud $@
} 

deploy() {
	cp ${ROOT}/package.json ${FNROOT}/$1/
	gcp beta functions deploy $1 --source ${FNROOT}/$1 --entry-point `echo $1 | sed -e s/-/_/g` --trigger-http
}

if [ -z "$1" ]; then
	# deploy all
	for f in `ls ${FNROOT}`; do
		if [ -d "${FNROOT}/$f" ]; then
			deploy $f
		fi
	done
else
	# deploy single
	if [ -e "${FNROOT}/$1" ]; then
		deploy $1
	else
		echo "$1: entry not found"
		exit 1
	fi
fi
