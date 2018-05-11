var MultiWalletProvider = require("../utils/multi-wallet");
var setting = require("./wallet");
module.exports = new MultiWalletProvider(setting);
