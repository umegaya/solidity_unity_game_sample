const p = require('./tools/settings/provider');

module.exports = {
  networks: {
    development: {
      //assume user uses ganache ethereum client with default setting
      provider: p,
      network_id: "*", // Match any network id
      gasLimit: 6000000,
    }
  }
};

/*var privateKey = "eb9bd3e2dfcf73e180c253dcdf8332eab5cdeb26b0f174cec3692b1681b93095";
// == address "0x56BB052E3C8dAD01F3bb0Fc8000a5E8475289849" */