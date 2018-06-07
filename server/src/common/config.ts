import {Factory} from './contract';
import * as fs from 'fs';
import * as mysql from 'mysql';
import * as helper from "./dapp/tools/utils/helper";

var cnf = process.env.CONFIG_NAME || "dev";
var config_set = {
    dev: {
        db: "mysql://root:namaham149@" + helper.getMinikubeHost() + "/data",
        rpc: {
            url: "http://" + helper.getMinikubeHost() + ":8545/",
            keystore:require("./secret/dev/user-1.ks"),
            pass:helper.chop(require("./secret/dev/user-1.pass")),
            addresses:require("./dapp/build/contracts/__dev__addresses__.json"),
        }
    },
    stage: {
        db: {
            connectionLimit: 1, 
            socketPath: '/cloudsql/' + 'gwixoss-dokyogames-01:us-central1:foodfighter-db',
            user     : 'root',
            password : 'namaham149',
            database: 'data'
        },
    },
    prod: {

    }
};
export var Config = config_set[cnf];
export var Contracts = Factory(Config.rpc.url, 
    Config.rpc.keystore, Config.rpc.pass, Config.rpc.addresses);
export var DbConn = mysql.createPool(Config.db);