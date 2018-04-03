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