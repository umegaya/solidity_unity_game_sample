const fs = require("fs");
const path = require("path");
const ethers = require("ethers");
const chop = require("./helper").chop;
const opts = require('../settings/wallet');

const getWallet = async () => {
    if (opts.sourceType == "parity_keys") {
        var passes = opts.passwords;
        var files = glob.sync(opts.sourcePath + "/UTC--*");
        if (files.length <= 0) {
            throw new Error("no parity key. abort");
        }
        var file = files[0];
        var key = fs.readFileSync(file);
        for (var j = 0; j < passes.length; j++) {
            try {
                return await ethers.Wallet.fromEncryptedWallet(key, passes[j]);
            } catch (e) {
                console.log(file + ":try recover with pass[" + pass + "] fails. continue");
            }
        }
    } else if (opts.sourceType == "ha-blockchain-tool") {
        var file = opts.sourcePath + "/user-1.ks";
        var key = fs.readFileSync(file);
        var passfile = path.dirname(file) + "/" + path.basename(file, ".ks") + ".pass";
        var pass = chop(fs.readFileSync(passfile).toString());
        return await ethers.Wallet.fromEncryptedWallet(key, pass);
    }
}

const createContract = async (truffle_contract, web3) => {
    const tmpc = await truffle_contract.deployed();
    const wallet = await getWallet();
    wallet.provider = new ethers.providers.Web3Provider(web3.currentProvider);
    return new ethers.Contract(tmpc.address, tmpc.abi, wallet);
}

module.exports = {
    getWallet: getWallet,
    Contract: createContract,
    overrideOptions: {
        gasLimit: 32000000,
    }
}