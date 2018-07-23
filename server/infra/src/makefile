# variables for execution of this makefile, given from parent
ACCOUNT=
SECRET=
REGION=
BUCKET=
NODETAG=
K8S_CONFIG_DIR=
PROJECT_ID=
OBJECT=

# variables for execution of this makefile, automatically decided
ROOT=`pwd`/..
SETUP_PATH=$(shell bash $(ROOT)/tools/decide_path.sh $(ROOT))
K8S_HOST=$(shell bash $(ROOT)/tools/kubehost.sh)
K8S_VARS=-var 'k8s_config_path=/k8s/config' -var 'k8s_host=$(K8S_HOST)'
SPEC_VARS=-var 'project_id=$(PROJECT_ID)' -var 'fw_node_filter=$(NODETAG)'
VARS=$(K8S_VARS) $(SPEC_VARS) -var 'access_key=$(ACCOUNT)' -var 'secret_key=$(SECRET)' -var 'region=$(REGION)' -var 'data_volume=$(STORAGE_VOLUME)' -var 'parity_config=$(CONFIG_VOLUME)' -var 'parity_secret=$(SECRET_VOLUME)' -var 'setup_path=$(SETUP_PATH)'
BACKEND_VARS=$(K8S_VARS) $(SPEC_VARS) -backend-config='access_key=$(ACCOUNT)' -backend-config='secret_key=$(SECRET)' -backend-config='bucket=$(BUCKET)' -backend-config='region=$(REGION)'

# variables for execution of this container
MOUNT=/tf
TFROOT=/tf/src
STORAGE_VOLUME=$(shell bash $(ROOT)/tools/decide_storage_path.sh)
CONFIG_VOLUME=$(MOUNT)/volume/config
SECRET_VOLUME=$(MOUNT)/volume/secret/$(K8S_PLATFORM)
WS=dev
PLAN=exec.tfplan
IMAGE=umegaya/terraform

define tf 
@[ ! -z "$(HOME)" ] || (echo "HOME is not set" && exit 1)
@[ ! -z "$(K8S_CONFIG_DIR)" ] || (echo "K8S_CONFIG_DIR is not set" && exit 1)
docker run --rm -ti --volumes-from gcloud-config -v $(HOME)/.minikube:$(HOME)/.minikube -v $(K8S_CONFIG_DIR):/k8s -v $(ROOT):$(MOUNT) -w $(TFROOT) $(IMAGE) $1 $2
endef

init:	
	@[ ! -z "$(BUCKET)" ] || (echo "BUCKET is not set" && exit 1)
	$(call tf,init $(BACKEND_VARS))

ws:
	$(call tf,workspace new $(WS))

select:
	$(call tf,workspace select $(VARS) $(WS))

plan:
	$(call tf,plan $(VARS) -out "$(TFROOT)/plans/$(PLAN)")

exec:
	$(call tf,apply "$(TFROOT)/plans/$(PLAN)")

apply:
	$(call tf,apply $(VARS))

taint:
	@[ ! -z "$(OBJECT)" ] || (echo "OBJECT is not set" && exit 1)
	$(call tf,taint $(VARS) $(OBJECT))

destroy:
	$(call tf,destroy $(VARS))
