resource "null_resource" "neko-blockchain-cm" {
  triggers {
    uuid = "${uuid()}"
  }
  provisioner "local-exec" {
    command = "kubectl create configmap neko-blockchain-cm --kubeconfig=/k8s/config -n neko --dry-run -o yaml --from-file=${var.parity_config} | kubectl apply --kubeconfig=/k8s/config -f -"
  }  
}

resource "null_resource" "neko-blockchain-secret" {
  triggers {
    uuid = "${uuid()}"
  }
  provisioner "local-exec" {
    command = "kubectl create secret generic neko-blockchain-secret --kubeconfig=/k8s/config -n neko --dry-run -o yaml --from-file=${var.parity_secret} | kubectl apply --kubeconfig=/k8s/config -f -"
  }  
}

