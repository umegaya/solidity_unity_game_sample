resource "null_resource" "ha-blockchain-cm" {
  triggers {
    uuid = "${uuid()}"
  }
  provisioner "local-exec" {
    command = "kubectl create configmap ha-blockchain-cm --kubeconfig=/k8s/config -n habc --dry-run -o yaml --from-file=${var.parity_config} | kubectl apply --kubeconfig=/k8s/config -f -"
  }  
}

resource "null_resource" "ha-blockchain-secret" {
  triggers {
    uuid = "${uuid()}"
  }
  provisioner "local-exec" {
    command = "kubectl create secret generic ha-blockchain-secret --kubeconfig=/k8s/config -n habc --dry-run -o yaml --from-file=${var.parity_secret} | kubectl apply --kubeconfig=/k8s/config -f -"
  }  
}
