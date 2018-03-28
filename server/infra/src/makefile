MOUNT=/tf
ACCOUNT=
SECRET=
REGIN=
BUCKET=
WS=dev
VARS=-var 'access_key=$(ACCOUNT)' -var 'secret_key=$(SECRET)' -var 'region=$(REGION)'
BACKEND_VARS=-backend-config='access_key=$(ACCOUNT)' -backend-config='secret_key=$(SECRET)' -backend-config='bucket=$(BUCKET)' -backend-config='region=$(REGION)'

define tf 
docker run --rm -ti -v `pwd`:$(MOUNT) -w $(MOUNT) hashicorp/terraform:light $1 $2
endef


init:	
	$(call tf,init $(BACKEND_VARS))

plan:
	$(call tf,plan $(VARS))

select:
	$(call tf,workspace select $(VARS) $(WS))
