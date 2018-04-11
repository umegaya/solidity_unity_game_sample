// persistent volumes
resource "kubernetes_persistent_volume" "neko-blockchain-shared-pv" {
    metadata {
        name = "neko-blockchain-shared-pv"
    }
    spec {
        capacity {
            storage = "16Mi"
        }
        storage_class_name = "standard"
        access_modes = ["ReadWriteOnce"]
        persistent_volume_reclaim_policy = "Retain"
        persistent_volume_source {
            host_path {
                path = "${var.shared_volume}"
            }
        }
    }
}

resource "kubernetes_persistent_volume" "neko-blockchain-data-pv" {
    metadata {
        name = "neko-blockchain-data-pv"
    }
    spec {
        capacity {
            storage = "3Gi"
        }
        storage_class_name = "standard"
        access_modes = ["ReadWriteOnce"]
        persistent_volume_reclaim_policy = "Retain"
        persistent_volume_source {
            host_path {
                path = "${var.data_volume}"
            }
        }
    }
}


// persistent volume claims
resource "kubernetes_persistent_volume_claim" "neko-blockchain-shared-pvc" {
    metadata {
      name = "neko-blockchain-shared-pvc"
      namespace = "neko"
    }
    spec {
      access_modes = ["ReadWriteOnce"]
      resources {
        requests {
          storage = "16Mi"
        }
      }
      volume_name = "${kubernetes_persistent_volume.neko-blockchain-shared-pv.metadata.0.name}"
  }
}

resource "kubernetes_persistent_volume_claim" "neko-blockchain-data-pvc" {
    metadata {
      name = "neko-blockchain-data-pvc"
      namespace = "neko"
    }
    spec {
      access_modes = ["ReadWriteOnce"]
      resources {
        requests {
          storage = "2Gi"
        }
      }
      volume_name = "${kubernetes_persistent_volume.neko-blockchain-data-pv.metadata.0.name}"
  }
}
