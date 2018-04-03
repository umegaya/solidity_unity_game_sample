ACCOUNT=
SECRET=
REGION=
BUCKET=
K8S_CONFIG_DIR=

IMAGE=umegaya/terraform

MOUNT=/tf
WS=dev
PLAN=exec.tfplan
K8S_VARS=-var 'k8s_config_path=/k8s/config'
VARS=$(K8S_VARS) -var 'access_key=$(ACCOUNT)' -var 'secret_key=$(SECRET)' -var 'region=$(REGION)'
BACKEND_VARS=$(K8S_VARS) -backend-config='access_key=$(ACCOUNT)' -backend-config='secret_key=$(SECRET)' -backend-config='bucket=$(BUCKET)' -backend-config='region=$(REGION)'

define tf 
# .minikube for local deployment
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

destroy:
	$(call tf,destroy $(VARS))
