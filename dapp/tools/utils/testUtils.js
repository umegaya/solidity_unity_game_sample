var fs = require("fs");
var path = require("path");
var glob = require("glob");
var keythereum = require("keythereum");
var PrivateKeyProvider = require("truffle-privatekey-provider");

var SECRET_ROOT = __dirname + "../../../server/infra/volume/secret";
var PROVIDERS = {};
var ADDRESSES = [];

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
            console.log("address:" + addr + " => pkey(" + pkstr + ")");
            PROVIDERS[addr] = new PrivateKeyProvider(pkstr, url);
            ADDRESSES.push(addr);
        }
    } else {
        glob(SECRET_ROOT + "/" + platform + "/*.ks", function (er, files) {
            for (var i = 0; i < files.length; i++) {
                var key = JSON.parse(fs.readFileSync(files[i]));
                var passfile = path.dirname(files[i]) + "/" + path.basename(files[i]) + ".pass";
                var pass = fs.readFileSync(passfile);
                var pkey = keythereum.recover(pass, key);
                if (pkey == null) {
                    throw new Error(files[i] + ":try recover with pass[" + pass + "] fails. abort");
                }
            }
        });
    }
    for (var k in ADDRESSES) {
        console.log("Addr[" + k + "]=" + ADDRESSES[k]);
    }
}

//use function to use arguments
var tx = function (contract, method) {
    var found = false;
    for (var i = 2; i < arguments.length; i++) {
        var arg = arguments[i];
        if (typeof(arg) == "object") {
            //should contain from
            var prov = PROVIDERS[arg.from];
            if (prov) {
                contract.setProvider(prov);
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
    return contract[method](arguments.slice(2));
}

var addresses = (index) => {
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

module.exports = {
    init: init,
    addresses: addresses,
    providers: providers,
    tx: tx,
}
