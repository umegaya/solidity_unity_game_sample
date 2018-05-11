import * as Web3 from "web3";
import * as fs from "fs";
var ProviderFactory = require('./provider');

function CreateContract(web3: Web3, abi_string: string, addr: string):Web3.ContractInstance {
    var abi: Web3.ContractAbi = JSON.parse(abi_string)
    return web3.eth.contract(abi).at(addr);
}

export class ContractSet {
    web3: Web3;
    World: Web3.ContractInstance;
    Inventory: Web3.ContractInstance;
    Token: Web3.ContractInstance;
}
export function Factory(url: string, key_store: string, pass: string, addrs:{[name:string]:string}):ContractSet {
    var provider: Web3.Provider = ProviderFactory(url, key_store, pass);
    var web3: Web3 = new Web3(provider);
    return {
        web3: web3,
        World: CreateContract(web3, require('./dapp/build/contracts/World.json'), addrs.World),
        Inventory: CreateContract(web3, require('./dapp/build/contracts/Inventory.json'), addrs.Inventory),
        Token: CreateContract(web3, require('./dapp/build/contracts/Moritapo.json'), addrs.Moritapo),
    };
}
