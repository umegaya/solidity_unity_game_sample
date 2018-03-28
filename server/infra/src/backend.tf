terraform {
  backend "s3" {
    key = "file.tfstate"
  }
}
