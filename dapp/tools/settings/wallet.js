var stage = "local";
var walletSettings = {
  dev: {
    url: "http://192.168.99.101:8545/",
    sourceType: "ha-blockchain-tool",
    sourcePath: __dirname + "/../../../server/infra/volume/secret/" + stage,      
  },
  local: {
    url: "http://localhost:8540/",
    sourceType: "parity_keys",
    sourcePath: "/tmp/parity0/keys/DemoPoA/", 
    passwords: ["node0", "node1", "user", "user2", "user3"],
  }
}

module.exports = walletSettings[stage];