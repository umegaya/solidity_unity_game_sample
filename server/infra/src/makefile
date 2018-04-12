ACCOUNT=
SECRET=
REGION=
BUCKET=
K8S_CONFIG_DIR=
OBJECT=
VOLUME=

IMAGE=umegaya/terraform

MOUNT=/tf
STORAGE=/var/parity
CONFIG=$(shell bash ../tools/decide_path.sh ..)	
WS=dev
PLAN=exec.tfplan
K8S_HOST=$(shell bash ../tools/kubehost.sh)
K8S_VARS=-var 'k8s_config_path=/k8s/config'
VARS=$(K8S_VARS) -var 'access_key=$(ACCOUNT)' -var 'secret_key=$(SECRET)' -var 'region=$(REGION)' -var 'shared_volume=$(VOLUME)' -var 'data_volume=$(STORAGE)' -var 'k8s_host=$(K8S_HOST)' -var 'parity_config=$(CONFIG)'
BACKEND_VARS=$(K8S_VARS) -backend-config='access_key=$(ACCOUNT)' -backend-config='secret_key=$(SECRET)' -backend-config='bucket=$(BUCKET)' -backend-config='region=$(REGION)'

define tf 
@[ ! -z "$(CONFIG)" ] || (echo "CONFIG is not set" && exit 1)
@[ ! -z "$(HOME)" ] || (echo "HOME is not set" && exit 1)
@[ ! -z "$(K8S_CONFIG_DIR)" ] || (echo "K8S_CONFIG_DIR is not set" && exit 1)
docker run --rm -ti --volumes-from gcloud-config -v $(HOME)/.minikube:$(HOME)/.minikube -v $(K8S_CONFIG_DIR):/k8s -v `pwd`:$(MOUNT) -w $(MOUNT) $(IMAGE) $1 $2
endef


init:	
	$(call tf,init $(BACKEND_VARS))

ws:
	$(call tf,workspace new $(WS))

select:
	$(call tf,workspace select $(VARS) $(WS))

plan:
	$(call tf,plan $(VARS) -out "./plans/$(PLAN)")

exec:
	$(call tf,apply "./plans/$(PLAN)")

apply:
	$(call tf,apply $(VARS))

taint:
	$(call tf,taint $(VARS) $(OBJECT))

destroy:
	$(call tf,destroy $(VARS))
