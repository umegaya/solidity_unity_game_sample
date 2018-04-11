provider "kubernetes" {
  config_path = "${var.k8s_config_path}"
}

resource "kubernetes_namespace" "neko" {
  metadata {
    name = "neko"
  }
}

resource "kubernetes_replication_controller" "neko-blockchain-rc" {
  metadata {
    namespace = "neko"
    name = "neko-blockchain-rc"
    labels {
      name = "neko-blockchain-node"
    }
  }

  spec {
    replicas = 1
    selector {
      name = "neko-blockchain-node"
    }
    template {
      container {
        image = "parity/parity"
        name  = "neko-blockchain-node"
        args = ["--config", "/shared/config/node0.toml"]
        volume_mount {
          name = "neko-blockchain-shared-volume"
          mount_path = "/shared"
        }
        volume_mount {
          name = "neko-blockchain-data-volume"
          mount_path = "/data"
        }
      }
      volume {
        name = "neko-blockchain-shared-volume"
        persistent_volume_claim {
          claim_name = "${kubernetes_persistent_volume_claim.neko-blockchain-shared-pvc.metadata.0.name}"
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
