#!/bin/bash

if [ "${K8S_PLATFORM}" = "dev" ]; then
    echo "/data"
else
    echo "/var"
fi
