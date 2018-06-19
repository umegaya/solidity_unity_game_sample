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
                addresses:require("./dapp/build/contracts/__dev__addresses__.json"),
            }
        }
    },
    stage: function () {
        return {
            rpc: {

            }
        }
    },
    prod: function () {
        return {
            rpc: {

            }
        }
    }
};
export var Config = config_set[cnf]();
export var Contracts = Factory(Config.rpc.url, 
    Config.rpc.keystore, Config.rpc.pass, Config.rpc.addresses);
