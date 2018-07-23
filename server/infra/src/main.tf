provider "kubernetes" {
  config_path = "${var.k8s_config_path}"
}

resource "kubernetes_namespace" "habc" {
  metadata {
    name = "habc"
  }
}

resource "kubernetes_daemonset" "ha-blockchain-ds" {
  depends_on = ["null_resource.ha-blockchain-cm", "null_resource.ha-blockchain-secret"]

  metadata {
    namespace = "habc"
    name = "ha-blockchain-ds"
    labels {
      name = "ha-blockchain-node"
    }
  }

  spec {
    selector {
      name = "ha-blockchain-node"
    }
    strategy {
      type = "RollingUpdate"
    }
    template {
      metadata {
        labels {
          name = "ha-blockchain-node"
        }
      }
      spec {
        container {
          image = "umegaya/parity"
          name  = "ha-blockchain-node"
          command = ["/bin/bash"]
          args = ["-c", "/parity/parity --config /shared/config/${var.setup_path}"]
          volume_mount {
            name = "ha-blockchain-config-volume"
            mount_path = "/shared/config"
          }
          volume_mount {
            name = "ha-blockchain-secret-volume"
            mount_path = "/shared/secret"
          }
          volume_mount {
            name = "ha-blockchain-data-volume"
            mount_path = "/data"
          }
          volume_mount {
            name = "ha-blockchain-etc-volume"
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
          name = "ha-blockchain-config-volume"
          config_map {
            name = "ha-blockchain-cm"
          }
        }
        volume {
          name = "ha-blockchain-secret-volume"
          secret {
            secret_name = "ha-blockchain-secret"
          }
        }
        volume {
          name = "ha-blockchain-data-volume"
          persistent_volume_claim {
            claim_name = "${kubernetes_persistent_volume_claim.ha-blockchain-data-pvc.metadata.0.name}"
          }
        }
        volume {
          name = "ha-blockchain-etc-volume"
          persistent_volume_claim {
            claim_name = "${kubernetes_persistent_volume_claim.ha-blockchain-etc-pvc.metadata.0.name}"
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
