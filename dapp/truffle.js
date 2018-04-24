var t = require(__dirname + "/tools/utils/testUtils");
t.init("poa-tutorial", "http://localhost:8540/");
/*var privateKey = "eb9bd3e2dfcf73e180c253dcdf8332eab5cdeb26b0f174cec3692b1681b93095";
// == address "0x56BB052E3C8dAD01F3bb0Fc8000a5E8475289849" */

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
      provider: t.providers(0), //new PrivateKeyProvider(privateKey, "http://localhost:8540/"),
      network_id: "*", // Match any network id
      gas: 4600000,
    }
  }
};
