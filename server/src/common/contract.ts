const Web3Class = require("web3")
import Web3 from 'web3/index.d'
import * as Web3Types from 'web3/types.d'
import * as fs from "fs";
var ProviderFactory = require('./provider');

function CreateContract(web3: Web3, contract_json: any, addr: string):Web3Types.Contract {
    return new web3.eth.Contract(contract_json.abi, addr);
}

export class ContractSet {
    web3: Web3;
    World: Web3Types.Contract;
    Inventory: Web3Types.Contract;
    Token: Web3Types.Contract;
}
export function Factory(url: string, key_store: string, pass: string, addrs:{[name:string]:string}):ContractSet {
    var provider: Web3Types.HttpProvider = ProviderFactory(url, key_store, pass);
    var web3: Web3 = new Web3Class(provider);
    return {
        web3: web3,
        World: CreateContract(web3, require('./dapp/build/contracts/World.json'), addrs.World),
        Inventory: CreateContract(web3, require('./dapp/build/contracts/Inventory.json'), addrs.Inventory),
        Token: CreateContract(web3, require('./dapp/build/contracts/Moritapo.json'), addrs.Moritapo),
    };
}
