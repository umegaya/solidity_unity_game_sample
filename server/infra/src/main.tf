provider "kubernetes" {
  config_path = "${var.k8s_config_path}"
}

resource "kubernetes_namespace" "neko" {
  metadata {
    name = "neko"
  }
}
