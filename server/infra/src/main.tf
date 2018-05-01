provider "kubernetes" {
  config_path = "${var.k8s_config_path}"
}

resource "kubernetes_namespace" "neko" {
  metadata {
    name = "neko"
  }
}

resource "kubernetes_daemonset" "neko-blockchain-ds" {
  depends_on = ["null_resource.neko-blockchain-cm", "null_resource.neko-blockchain-secret"]

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
    strategy {
      type = "RollingUpdate"
    }
    template {
      metadata {
        labels {
          name = "neko-blockchain-node"
        }
      }
      spec {
        container {
          image = "umegaya/parity"
          name  = "neko-blockchain-node"
          command = ["/bin/bash"]
          args = ["-c", "/parity/parity --config /shared/config/${var.setup_path}"]
          volume_mount {
            name = "neko-blockchain-config-volume"
            mount_path = "/shared/config"
          }
          volume_mount {
            name = "neko-blockchain-secret-volume"
            mount_path = "/shared/secret"
          }
          volume_mount {
            name = "neko-blockchain-data-volume"
            mount_path = "/data"
          }
          volume_mount {
            name = "neko-blockchain-etc-volume"
            mount_path = "/hostetc"
          }
          port {
            name = "ethereum"
            container_port = 30303
            host_port = 30303            
          }
          port {
            name = "peer-discovery"
            container_port = 30303
            host_port = 30303
            protocol = "UDP"
          }
          port {
            name = "json-rpc"
            container_port = 8545
            host_port = 8545
          }
        }
        volume {
          name = "neko-blockchain-config-volume"
          config_map {
            name = "neko-blockchain-cm"
          }
        }
        volume {
          name = "neko-blockchain-secret-volume"
          secret {
            secret_name = "neko-blockchain-secret"
          }
        }
        volume {
          name = "neko-blockchain-data-volume"
          persistent_volume_claim {
            claim_name = "${kubernetes_persistent_volume_claim.neko-blockchain-data-pvc.metadata.0.name}"
          }
        }
        volume {
          name = "neko-blockchain-etc-volume"
          persistent_volume_claim {
            claim_name = "${kubernetes_persistent_volume_claim.neko-blockchain-etc-pvc.metadata.0.name}"
          }
        }
      }
    }
  }
}

module "specific" {
  source = "./specific"

  project_id = "${var.project_id}"
  fw_node_filter = "${var.fw_node_filter}"
}
