## how to setup kube cluster
- before using terraform scripts under this directory, you need to setup kubernetes cluster manually.

### gcp
- use console https://console.cloud.google.com/kubernetes/
- ```make k8s PLATFORM=gcp```

### aws, azure
- use tectonic https://github.com/coreos/tectonic-installer
- ```make k8s PLATFORM=aws or azure```

### local(dev)
- use minikube https://github.com/kubernetes/minikube
- no need to make k8s

## commands
- basically short hand of terraform. parameter not specified, following default value will be used
  - PLATFORM => dev
  - WS => dev
  - PLAN => exec.tfplan
- ```make init PLATFORM=XXX``` terraform init. 
- ```make ws WS=XXX``` terraform workspace new.
- ```make select WS=XXX``` terraform workspace select.
- ```make plan PLATFORM=XXX PLAN=YYY``` terraform plan. will create plan file ./plans/YYY
- ```make exec PLATFORM=XXX PLAN=YYY``` terraform apply with plan file ./plans/YYY
- ```make apply PLATFORM=XXX``` terraform apply without plan file
- ```make destroy PLATFORM=XXX``` terraform destroy
