# ----------------------------------------------------------------------------------
# docker related
# ----------------------------------------------------------------------------------
image:
	@docker build -t umegaya/terraform docker


# ----------------------------------------------------------------------------------
# terraform related
# ----------------------------------------------------------------------------------
S3_BUCKET=dokyogames-terraform-bucket

WS=dev
PLAN=exec.tfplan
OBJECT=

PLATFORM=dev
ifeq ($(PLATFORM),dev)
K8S_CONFIG_DIR=$(HOME)/.kube
VOLUME=`pwd`/volume
else
K8S_CONFIG_DIR=`pwd`/k8s/$(PLATFORM)
# TODO
VOLUME=/path/to/volume
ifeq ($(PLATFORM),gcp) 
K8S_REGION=us-central1
K8S_CLUSTER=k8stest
else
endif
endif

# before this, you should configure your google/cloud-sdk container by
# gcloud config set container/use_v1_api false and $gcloud config set project PROJECT_ID
k8s:
	@make -C k8s $(PLATFORM) CLUSTER=$(K8S_CLUSTER) REGION=$(K8S_REGION)

init:
	@echo "if platform == dev, be sure that you already create directory /var/parity/storage in your minikube VM"
	@make -C src init ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) BUCKET=$(S3_BUCKET) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

ws:
	@make -C src ws ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) WS=$(WS) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

plan:
	@make -C src plan ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR) VOLUME=$(VOLUME)

select:
	@make -C src select ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) WS=$(WS) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

apply:
	@make -C src apply ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR) VOLUME=$(VOLUME)

exec:
	@make -C src exec PLAN=$(PLAN) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

taint:
	@make -C src taint ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR) OBJECT=$(OBJECT) VOLUME=$(VOLUME)

destroy:
	@make -C src destroy ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR) VOLUME=$(VOLUME)

.PHONY: k8s

# ----------------------------------------------------------------------------------
# administration related
# ----------------------------------------------------------------------------------
# TODO: automatically retrieve this information (via terraform output?)
POD=`kubectl get po -o json | jq -r .items[0].metadata.name`
KUBEHOST=`minikube ip`
COUNT=3

genesis: 
	@minikube ssh rm -r /var/parity/storage
	@minikube ssh mkdir -p /var/parity/storage
	@kubectl exec $(POD) kill 1

restart:
	@kubectl exec $(POD) kill 1

tail:
	@kubectl logs -f $(POD)	

sh:
	@kubectl exec -ti $(POD) /bin/bash

accounts:
	curl --data '{"jsonrpc":"2.0","method":"parity_newAccountFromPhrase","params":["node0", "node0"],"id":0}' -H "Content-Type: application/json" -X POST $(KUBEHOST):30540

















