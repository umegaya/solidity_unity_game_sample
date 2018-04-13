DOCKER_OPTS=-e 'KUBECONFIG=./gcp/config'
REGION=
CLUSTER=
PROJECT_ID=

define gcloud
docker run --rm -ti -v `pwd`:/work -w /work --volumes-from gcloud-config --entrypoint=gcloud $(DOCKER_OPTS) umegaya/terraform $1
endef

gcp_setup:
	docker inspect gcloud-config || docker run -ti --name gcloud-config --entrypoint=gcloud umegaya/terraform auth application-default login
	$(call gcloud,config set container/use_v1_api false)
	$(call gcloud,config set project $(PROJECT_ID))

gcp:
	$(call gcloud,beta container clusters get-credentials --region $(REGION) $(CLUSTER))

aws:
	echo "TODO: get credential of aws k8s clusters" && exit 1

azure:
	echo "TODO: get credential of azure k8s clusters" && exit 1

.PHONY: gcp aws azure

