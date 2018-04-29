var MultiWalletProvider = require(__dirname + "/../utils/multi-wallet");
var setting = require("./wallet.js");
module.exports = new MultiWalletProvider(setting);
