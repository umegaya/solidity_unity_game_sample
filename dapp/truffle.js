module.exports = {
  networks: {
    development: {
      //assume user uses ganache ethereum client with default setting
      host: "127.0.0.1", //"192.168.99.101", 
      port: 8540,//9545, 
      network_id: "*", // Match any network id
      gas: 4600000,
    }
  }
};
