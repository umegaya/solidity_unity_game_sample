module.exports = {
  networks: {
    development: {
      //assume user uses ganache ethereum client with default setting
      host: "192.168.99.102",
      port: 30545, 
      network_id: "*", // Match any network id
      gas: 4600000,
    }
  }
};
