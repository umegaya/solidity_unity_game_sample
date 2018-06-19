var Web3Ctor = require('web3');
import Web3Class from 'web3/index';
import * as Web3 from 'web3/types';
import ProviderFactory = require('./provider');

function CreateContract(web3: Web3Class, contract_json: any, addr: string):Web3.Contract {
    return new web3.eth.Contract(contract_json.abi, addr);
}

export class ContractSet {
    web3: Web3Class;
    World: Web3.Contract;
    Inventory: Web3.Contract;
    Token: Web3.Contract;
}
export function Factory(url: string, key_store: string, pass: string, addrs:{[name:string]:string}):ContractSet {
    var provider: Web3.HttpProvider = null; //ProviderFactory(url, key_store, pass);
    var web3: Web3Class = new Web3Ctor(provider);
    return {
        web3: web3,
        World: CreateContract(web3, require('./dapp/build/contracts/World.json'), addrs.World),
        Inventory: CreateContract(web3, require('./dapp/build/contracts/Inventory.json'), addrs.Inventory),
        Token: CreateContract(web3, require('./dapp/build/contracts/Moritapo.json'), addrs.Moritapo),
    };
}
