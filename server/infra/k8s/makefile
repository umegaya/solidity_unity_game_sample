DOCKER_OPTS=-e 'KUBECONFIG=./gcp/config'
REGION=
CLUSTER=

define gcloud
docker run --rm -ti -v `pwd`:/work -w /work --volumes-from gcloud-config $(DOCKER_OPTS) google/cloud-sdk gcloud $1
endef

gcp:
	docker inspect gcloud-config || docker run -ti --name gcloud-config google/cloud-sdk gcloud auth login
	$(call gcloud,beta container clusters get-credentials --region $(REGION) $(CLUSTER))

aws:
	echo "TODO: get credential of aws k8s clusters" && exit 1

azure:
	echo "TODO: get credential of azure k8s clusters" && exit 1

.PHONY: gcp aws azure

