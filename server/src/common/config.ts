import {Factory} from './contract';
import * as helper from "./dapp/tools/utils/helper";

var cnf = process.env.CONFIG_NAME || "dev";
var config_set = {
    dev: function () {
        return {
            rpc: {
                url: "http://" + helper.getMinikubeHost() + ":8545/",
                keystore:require("./secret/dev/user-1.ks"),
                pass:helper.chop(require("./secret/dev/user-1.pass")),
                addresses:require("./dapp/build/addresses/dev.json"),
            },
            wallet_address: helper.chop(require("./secret/dev/user-1.addr")),
        };
    },
    stage: function () {
        return {
            /*rpc: {
                url: "http://" + helper.getMinikubeHost() + ":8545/",
                keystore:require("./secret/gcp/user-1.ks"),
                pass:helper.chop(require("./secret/gcp/user-1.pass")),
                addresses:require("./dapp/build/addresses/gcp.json"),
            },
            wallet_address: helper.chop(require("./secret/gcp/user-1.addr")),*/
        }
    },
    prod: {

    }
};
export var Config = config_set[cnf]();
export var Contracts = Factory(Config.rpc.url, 
    Config.rpc.keystore, Config.rpc.pass, Config.rpc.addresses, {
        from: Config.wallet_address, 
        gas: 4000000,
    });
