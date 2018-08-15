var exec = require('child_process').execSync;
var helper = require('../utils/helper');

var stage = process.env.CONFIG_NAME || "dev";
var walletSettings = {
  dev: {
    url: "http://" + helper.chop(exec("minikube ip")) + ":8545/",
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