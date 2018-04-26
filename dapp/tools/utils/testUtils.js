const fs = require("fs");
const path = require("path");
const glob = require("glob");
const keythereum = require("keythereum");
const PrivateKeyProvider = require("truffle-privatekey-provider");
const HookedWalletProvider = require('web3-provider-engine/subproviders/hooked-wallet-ethtx');


var PROVIDERS = {};
var ADDRESSES = [];
var CONTEXT = {};

var chop = (str) => {
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

var init = (platform, url) => {
    console.log("init accounts for " + platform + ", url(" + url + ")");
    if (platform == "poa-tutorial") {
        var passes = ["node0", "node1", "user", "user2", "user3"];
        var files = glob.sync("/tmp/parity0/keys/DemoPoA/UTC--*");
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
            var pkstr = pkey.toString('hex');
            //console.log("address:" + addr + " => pkey(" + pkstr + ")");
            PROVIDERS[addr] = new PrivateKeyProvider(pkstr, url);
            ADDRESSES.push(addr);
        }
    } else {
		var SECRET_ROOT = __dirname + "/../../../server/infra/volume/secret";
		var pattern = SECRET_ROOT + "/" + platform + "/*.ks";
        var files = glob.sync(pattern);
        //console.log("files:" + files.length + ",pattern:" + pattern);
        for (var i = 0; i < files.length; i++) {
            var key = JSON.parse(fs.readFileSync(files[i]));
            var passfile = path.dirname(files[i]) + "/" + path.basename(files[i], ".ks") + ".pass";
            var pass = chop(fs.readFileSync(passfile).toString());
            var pkey = keythereum.recover(pass, key);
            if (pkey == null) {
                throw new Error(files[i] + ":try recover with pass[" + pass + "] fails. abort");
            }
            var addr = keythereum.privateKeyToAddress(pkey);
            var pkstr = pkey.toString('hex');
            //console.log("address:" + addr + " => pkey(" + pkstr + ")");
            PROVIDERS[addr] = new PrivateKeyProvider(pkstr, url);
            ADDRESSES.push(addr);
        }
    }
    for (var k in ADDRESSES) {
        console.log("Addr[" + k + "]=" + ADDRESSES[k]);
    }
}

//use function to use arguments
var tx = function (tmpl, contract, method) {
    var found = false;
    for (var i = 2; i < arguments.length; i++) {
        var arg = arguments[i];
        if (typeof(arg) == "object") {
            //should contain from
            var prov = PROVIDERS[arg.from];
            if (prov) {
                if (prov.address != arg.from) {
                    console.log("invalid provider used:" + prov.for_addr + " != " + arg.from);
                    for (var k in PROVIDERS) {
                        console.log(k, PROVIDERS[k]);
                    }
                }
                tmpl.setProvider(prov);
            } else {
                throw new Error("provider not initialized for address:" + arg.from);
            }
            found = true;
            break;
        }
    }
    if (!found) {
        throw new Error("to use tx, need to pass setting objet { from:..., ... }");        
    }
    return contract[method](...(Array.from(arguments).slice(3)));
}

var address = (index) => {
    var a = ADDRESSES[index];
    if (!a) {
        throw new Error("address not found for " + index);
    }
    return a;
}
var providers = (index) => {
    var p = PROVIDERS[ADDRESSES[index]];
    if (!p) {
        throw new Error("provider should initialized for " + ADDRESSES[index]);
    }
    return p;
}
var set_web3 = (instance) => {
    CONTEXT.web3 = instance;
}

module.exports = {
    init: init,
    set_web3: set_web3,
    addresses: ADDRESSES,
    address: address,
    providers: providers,
    tx: tx,
}
