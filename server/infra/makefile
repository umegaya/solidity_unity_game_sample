# ----------------------------------------------------------------------------------
# docker related
# ----------------------------------------------------------------------------------
image:
	@docker build -t umegaya/terraform docker


# ----------------------------------------------------------------------------------
# terraform related
# ----------------------------------------------------------------------------------
S3_BUCKET=dokyogames-terraform-bucket

WS=$(K8S_PLATFORM)
PLAN=exec.tfplan
OBJECT=kubernetes_daemonset.neko-blockchain-ds

PLATFORM=$(K8S_PLATFORM)
PROJECT_ID=
ifeq ($(PLATFORM),dev)
K8S_CONFIG_DIR=$(HOME)/.kube
else
K8S_CONFIG_DIR=`pwd`/k8s/$(PLATFORM)
ifeq ($(PLATFORM),gcp) 
K8S_REGION=us-central1
K8S_CLUSTER=neko
else
endif
endif

# for gcp, project_id is your project id. 
# TODO: document other cloud services
k8s_setup:
	@make -C k8s $(PLATFORM)_setup CLUSTER=$(K8S_CLUSTER) REGION=$(K8S_REGION) PROJECT_ID=$(PROJECT_ID)

k8s:
	@make -C k8s $(PLATFORM) CLUSTER=$(K8S_CLUSTER) REGION=$(K8S_REGION)

init:
	@echo "if platform == dev, be sure that you already create directory /var/parity/storage in your minikube VM"
	@make -C src init ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) BUCKET=$(S3_BUCKET) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

ws:
	@make -C src ws ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) WS=$(WS) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

plan:
	@make -C src plan ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

select:
	@make -C src select ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) WS=$(WS) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

apply:
	@make -C src apply ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

exec:
	@make -C src exec PLAN=$(PLAN) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

taint:
	@make -C src taint ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR) OBJECT=$(OBJECT)

destroy:
	@make -C src destroy ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

.PHONY: k8s

# ----------------------------------------------------------------------------------
# administration related
# ----------------------------------------------------------------------------------
POD=`kcd get po -o json | jq -r .items[0].metadata.name`
KUBEHOST=`bash ./tools/kubehost.sh`
CREDENTIAL_OUT=`pwd`/volume/secret

genesis: chain_spec taint plan exec

restart:
	@kubectl exec $(POD) kill 1

tail:
	@kubectl logs -f $(POD)	

sh:
	@kubectl exec -ti $(POD) /bin/bash

chain_spec:
	@bash ./tools/create_mesh.sh
	@bash ./tools/make_account.sh $(KUBEHOST)
	@bash ./tools/make_chain_spec.sh










