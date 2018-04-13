#!/bin/bash

INFRA_PROJECT_ROOT=${1}

kcd() {
	local kconfig=
	case "${K8S_PLATFORM}" in
	"dev") kconfig="/k8s_local/config";;
	"gcp") kconfig="/k8s/gcp/config";;
	"aws") kconfig="/k8s/aws/config";;
	"azure") kconfig="/k8s/azure/config";;
	*) echo "${K8S_PLATFORM}: no match!!" && exit 1;;
	esac
	docker run --rm -ti --entrypoint=kubectl --volumes-from=gcloud-config -v ${HOME}/.minikube:${HOME}/.minikube \
		-v ${INFRA_PROJECT_ROOT}/k8s:/k8s -v ${HOME}/.kube:/k8s_local \
		-e "KUBECONFIG=${kconfig}" umegaya/terraform $@
}

node_list() {
	local iftype="ExternalIP"
	if [ "${K8S_PLATFORM}" = "dev" ]; then
		iftype="InternalIP"
	fi
	kcd get node -o json | jq -r .items[] \
		| jq -r "{interface:.status.addresses[],machineID:.status.nodeInfo.machineID}" \
		| jq -r "select(.interface.type == \"${iftype}\")" \
		| jq -r "{address:.interface.address,machineID:.machineID}"
}
