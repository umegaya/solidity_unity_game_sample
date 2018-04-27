var MultiWalletProvider = require(__dirname + "/tools/utils/multi-wallet");
var stage = "local";
var walletSettings = {
  dev: {
    url: "http://192.168.99.101:8545/",
    sourceType: "ha-blockchain-tool",
    sourcePath: __dirname + "/../server/infra/volume/secret/" + stage,      
  },
  local: {
    url: "http://localhost:8540/",
    sourceType: "parity_keys",
    sourcePath: "/tmp/parity0/keys/DemoPoA/", 
    passwords: ["node0", "node1", "user", "user2", "user3"],
  }
}
var p = new MultiWalletProvider(walletSettings[stage]);

module.exports = {
  networks: {
    k8s_local: {

    },
    k8s_gcp: {

    },
    test_failure: {
      host: "127.0.0.1",
      port: 8540,
      network_id: "*", // Match any network id
      gas: 4600000,       
    },
    development: {
      //assume user uses ganache ethereum client with default setting
      provider: p, //new PrivateKeyProvider(privateKey, "http://localhost:8540/"),
      network_id: "*", // Match any network id
      gas: 4600000,
    }
  }
};

/*var privateKey = "eb9bd3e2dfcf73e180c253dcdf8332eab5cdeb26b0f174cec3692b1681b93095";
// == address "0x56BB052E3C8dAD01F3bb0Fc8000a5E8475289849" */