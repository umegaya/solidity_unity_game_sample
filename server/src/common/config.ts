import {Factory} from './contract';
import * as fs from 'fs';

var cnf = process.env.CONFIG_NAME || "dev";
var config_set = {
    dev: {
        DATABASE:"mysql://root:namaham149@192.168.99.100/data",
        rpc: {
            url:"http://192.168.99.101:8545",
            keystore:require("./secret/dev/user-1.ks").toString(),
            pass:require("./secret/dev/user-1.pass").toString(),
            addresses: JSON.parse(require("./dapp/build/contracts/__dev__addresses__.json").toString()),
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
