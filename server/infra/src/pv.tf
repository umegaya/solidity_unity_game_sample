  // persistent volumes
resource "kubernetes_persistent_volume" "ha-blockchain-data-pv" {
  metadata {
    name = "ha-blockchain-data-pv"
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

resource "kubernetes_persistent_volume" "ha-blockchain-etc-pv" {
  metadata {
    name = "ha-blockchain-etc-pv"
  }
  spec {
    capacity {
      storage = "1Gi"
    }
    storage_class_name = "standard"
    access_modes = ["ReadOnlyMany"]
    persistent_volume_reclaim_policy = "Retain"
    persistent_volume_source {
      host_path {
        path = "/etc"
      }
    }
  }
}


// persistent volume claims
resource "kubernetes_persistent_volume_claim" "ha-blockchain-data-pvc" {
  metadata {
    name = "ha-blockchain-data-pvc"
    namespace = "habc"
  }
  spec {
    access_modes = ["ReadWriteOnce"]
    resources {
      requests {
        storage = "2Gi"
      }
    }
    volume_name = "${kubernetes_persistent_volume.ha-blockchain-data-pv.metadata.0.name}"
  }
}

resource "kubernetes_persistent_volume_claim" "ha-blockchain-etc-pvc" {
  metadata {
    name = "ha-blockchain-etc-pvc"
    namespace = "habc"
  }
  spec {
    access_modes = ["ReadOnlyMany"]
    resources {
      requests {
        storage = "1Gi"
      }
    }
    volume_name = "${kubernetes_persistent_volume.ha-blockchain-etc-pv.metadata.0.name}"
  }
}
