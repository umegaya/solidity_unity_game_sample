provider "aws" {
  access_key = "${var.access_key}"
  secret_key = "${var.secret_key}"
  region     = "${var.region}"
}

resource "aws_instance" "example" {
  tags {
    Name = "test - ${terraform.workspace}"
  }
  ami           = "ami-ceafcba8"
  instance_type = "t2.micro"
}
