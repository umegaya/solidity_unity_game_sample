resource "google_compute_firewall" "ha-blockchain-fw" {
  name    = "ha-blockchain-fw"
  network = "default"
  project = "${var.project_id}"

  allow {
    protocol = "tcp"
    ports    = ["30303", "8545"]
  }

  allow {
    protocol = "udp"
    ports    = ["30303"]
  }

  target_tags = ["${var.fw_node_filter}"]
  source_ranges = ["0.0.0.0/0"]
}

