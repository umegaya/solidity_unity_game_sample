#!/bin/bash

CONTEXT=`kubectl config current-context`
kubectl config view -o json | jq -r .clusters[] | jq -r "select(.name == \"${CONTEXT}\").cluster.server" | sed -e 's|https://\([^:]*\):.*$|\1|'
