const fs = require("fs");
const path = require("path");
const inherits = require('util').inherits
const glob = require("glob");
const keythereum = require("keythereum");
const Web3 = require("web3");
const ProviderEngine = require("web3-provider-engine");
const FiltersSubprovider = require('web3-provider-engine/subproviders/filters.js');
const WalletSubprovider = require('web3-provider-engine/subproviders/wallet.js');
const Web3Subprovider = require("web3-provider-engine/subproviders/web3.js");
const NonceSubprovider = require('web3-provider-engine/subproviders/nonce-tracker.js');
const HookedWalletEthTxSubprovider = require('web3-provider-engine/subproviders/hooked-wallet-ethtx');

inherits(MultiWalletEthTxProvider, HookedWalletEthTxSubprovider);

function MultiWalletEthTxProvider(opts) {    
    const self = this;
    self.privkeys_ = {};
    self.addresses_ = [];
    opts.getAccounts = opts.getAccounts || function (cb) {
        cb(null, self.addresses_);
    };
    opts.getPrivateKey = opts.getPrivateKey || function (from, cb) {
        const pkey = self.privkeys_[from];
        if (!pkey) {
            cb(new Error("pkey for " + from + " not found"), null);
            return;
        }
        cb(null, pkey);
    };
    self.init(opts);
    MultiWalletEthTxProvider.super_.call(self, opts);
}

MultiWalletEthTxProvider.chop = function (str) {
	var i = str.length - 1;
	for (; i >= 0; i--) {
		if (str.charAt(i) != '\r' && str.charAt(i) != '\n') {
			break;
		}
	}
	if (i < (str.length - 1)) {
		return str.substring(0, i+1);
	} else {
		return str;
	}
}

MultiWalletEthTxProvider.prototype.init = function (opts) {
    const self = this;
    console.log("init accounts for " + opts.sourceType + ", vault(" + opts.sourcePath + "), url(" + opts.url + ")");
    if (opts.sourceType == "parity_keys") {
        var passes = opts.passwords;
        var files = glob.sync(opts.sourcePath + "/UTC--*");
        for (var i = 0; i < files.length; i++) {
            var key = JSON.parse(fs.readFileSync(files[i]));
            for (var j = 0; j < passes.length; j++) {
                var pass = passes[j];
                var pkey = null;
                try {
                    pkey = keythereum.recover(pass, key);
                    if (!pkey) { 
                        console.log(files[i] + ":try recover with pass[" + pass + "] fails. continue");
                        continue; 
                    }
                    break;
                } catch (e) {
                    console.log(files[i] + ":try recover with pass[" + pass + "] fails(throw). continue");
                }
            }
            if (!pkey) {
                throw new Error(files[i] + ":try recover with pass fails. abort");
            }
            var addr = keythereum.privateKeyToAddress(pkey);
            //console.log("address:" + addr + " => pkey(" + pkstr + ")");
            self.privkeys_[addr] = pkey;
            self.addresses_.push(addr);
        }
    } else if (opts.sourceType == "ha-blockchain-tool") {
		var pattern = opts.sourcePath + "/*.ks";
        var files = glob.sync(pattern);
        //console.log("files:" + files.length + ",pattern:" + pattern);
        for (var i = 0; i < files.length; i++) {
            var key = JSON.parse(fs.readFileSync(files[i]));
            var passfile = path.dirname(files[i]) + "/" + path.basename(files[i], ".ks") + ".pass";
            var pass = MultiWalletEthTxProvider.chop(fs.readFileSync(passfile).toString());
            var pkey = keythereum.recover(pass, key);
            if (pkey == null) {
                throw new Error(files[i] + ":try recover with pass[" + pass + "] fails. abort");
            }
            var addr = keythereum.privateKeyToAddress(pkey);
            //console.log("address:" + addr + " => pkey(" + pkstr + ")");
            self.privkeys_[addr] = pkey;
            self.addresses_.push(addr);
        }
    }
    for (var k in self.addresses_) {
        console.log("addr[" + k + "]=" + self.addresses_[k]);
    }
}

function MultiWalletProvider(opts) {
    this.engine = new ProviderEngine();

    this.engine.addProvider(new FiltersSubprovider());
    this.engine.addProvider(new NonceSubprovider());
    var wallet = new MultiWalletEthTxProvider(opts);
    this.engine.addProvider(wallet);
    this.addresses_ = wallet.addresses_;
    this.engine.addProvider(new Web3Subprovider(new Web3.providers.HttpProvider(opts.url)));
    this.engine.start();
}

MultiWalletProvider.prototype.sendAsync = function() {
    this.engine.sendAsync.apply(this.engine, arguments);
};
  
MultiWalletProvider.prototype.send = function() {
    return this.engine.send.apply(this.engine, arguments);
};  
  

module.exports = MultiWalletProvider;
