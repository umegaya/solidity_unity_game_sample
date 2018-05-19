import {Factory} from './contract';
import * as fs from 'fs';
import {execSync} from 'child_process';
import * as helper from "./dapp/tools/utils/helper";

var cnf = process.env.CONFIG_NAME || "dev";
var config_set = {
    dev: {
        DATABASE:"mysql://root:namaham149@192.168.99.100/data",
        rpc: {
            url: "http://" + helper.chop(execSync("minikube ip")) + ":8545/",
            keystore:require("./secret/dev/user-1.ks"),
            pass:helper.chop(require("./secret/dev/user-1.pass")),
            addresses:require("./dapp/build/contracts/__dev__addresses__.json"),
        }
    },
    stage: {

    },
    prod: {

    }
};
export var Config = config_set[cnf];
export var Contracts = Factory(Config.rpc.url, 
    Config.rpc.keystore, Config.rpc.pass, Config.rpc.addresses);
