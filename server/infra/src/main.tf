provider "kubernetes" {
  config_path = "${var.k8s_config_path}"
}

resource "kubernetes_namespace" "neko" {
  metadata {
    name = "neko"
  }
}

resource "null_resource" "neko-blockchain-cm" {
  provisioner "local-exec" {
    command = "kubectl create configmap neko-blockchain-cm --kubeconfig=/k8s/config -n neko --dry-run -o yaml --from-file=${var.parity_config} | kubectl apply --kubeconfig=/k8s/config -f -"
  }  
}

resource "kubernetes_daemonset" "neko-blockchain-ds" {
  depends_on = ["null_resource.neko-blockchain-cm"]

  metadata {
    namespace = "neko"
    name = "neko-blockchain-ds"
    labels {
      name = "neko-blockchain-node"
    }
  }

  spec {
    selector {
      name = "neko-blockchain-node"
    }
    template {
      metadata {
        labels {
          name = "neko-blockchain-node"
        }
      }
      spec {
        container {
          image = "parity/parity"
          name  = "neko-blockchain-node"
          args = ["--config", "/shared/config/${var.setup_path}"]
          volume_mount {
            name = "neko-blockchain-config-volume"
            mount_path = "/shared/config"
          }
          volume_mount {
            name = "neko-blockchain-data-volume"
            mount_path = "/data"
          }
        }
        volume {
          name = "neko-blockchain-config-volume"
          config_map {
            name = "neko-blockchain-cm"
          }
        }
        volume {
          name = "neko-blockchain-data-volume"
          persistent_volume_claim {
            claim_name = "${kubernetes_persistent_volume_claim.neko-blockchain-data-pvc.metadata.0.name}"
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "neko-blockchain-service" {
  metadata {
    namespace = "neko"
    name = "neko-blockchain-service"
  }
  spec {
    selector {
      name = "neko-blockchain-node"
    }
    session_affinity = "ClientIP"
    port {
      name = "ethereum-protocol"
      port = 30303
      node_port = 30303
    }
    port {
      name = "json-rpc"
      port = 8545
      node_port = 30545
    }
    port {
      name = "peer-discovery"
      port = 30303
      node_port = 30303
      protocol = "UDP"
    }

    type = "NodePort"
  }
}
