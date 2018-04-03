S3_BUCKET=dokyogames-terraform-bucket

WS=dev
PLAN=exec.tfplan

PLATFORM=dev
ifeq ($(PLATFORM),dev)
K8S_CONFIG_DIR=$(HOME)/.kube
else
K8S_CONFIG_DIR=`pwd`/k8s/$(PLATFORM)
ifeq ($(PLATFORM),gcp)
K8S_REGION=us-central1
K8S_CLUSTER=k8stest
else
endif
endif


image:
	@docker build -t umegaya/terraform tool

# before this, you should configure your google/cloud-sdk container by
# gcloud config set container/use_v1_api false and $gcloud config set project PROJECT_ID
k8s:
	@make -C k8s $(PLATFORM) CLUSTER=$(K8S_CLUSTER) REGION=$(K8S_REGION)

init:
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
	@make -C src exec PLAN=$(PLAN)

destroy:
	@make -C src destroy ACCOUNT=$(AWS_ACCESS_KEY) SECRET=$(AWS_SECRET_KEY) REGION=$(AWS_DEFAULT_REGION) K8S_CONFIG_DIR=$(K8S_CONFIG_DIR)

.PHONY: k8s