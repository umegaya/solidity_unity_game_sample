const Web3 = require("web3");
const p = require(__dirname + "/../settings/provider");

web3 = new Web3(p);

web3.eth.sendTransaction({
    from: p.addresses_[0],
    to: process.argv[2],
    value: web3.toWei(process.argv[3], "ether"),
}, (err, hash) => {
    console.log(err, hash);
});
