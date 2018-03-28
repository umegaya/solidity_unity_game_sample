MOUNT=/tf
ACCOUNT=
SECRET=
REGIN=
BUCKET=
WS=dev
PLAN=exec.plan
VARS=-var 'access_key=$(ACCOUNT)' -var 'secret_key=$(SECRET)' -var 'region=$(REGION)'
BACKEND_VARS=-backend-config='access_key=$(ACCOUNT)' -backend-config='secret_key=$(SECRET)' -backend-config='bucket=$(BUCKET)' -backend-config='region=$(REGION)'

define tf 
docker run --rm -ti -v `pwd`:$(MOUNT) -w $(MOUNT) hashicorp/terraform:light $1 $2
endef


init:	
	$(call tf,init $(BACKEND_VARS))

plan:
	$(call tf,plan $(VARS) -out "./plans/exec.plan")

select:
	$(call tf,workspace select $(VARS) $(WS))

apply:
	$(call tf,apply $(VARS) "./plans/$(PLAN)")

exec:
	$(call tf,apply "./plans/$(PLAN)")