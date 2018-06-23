var Web3Ctor = require('web3');
import Web3Class from 'web3/index';
import * as Web3 from 'web3/types';
import ProviderFactory = require('./provider');

function CreateContract(web3: Web3Class, contract_json: any, addr: string, default_setting: TxSetting):Web3.Contract {
    return new web3.eth.Contract(contract_json.abi, addr, default_setting);
}

export class TxSetting {
    from: string;
    gas: number | string;
    gasPrice?: number;
    data?: string;
}

export class ContractSet {
    web3: Web3Class;
    instances:{[label:string]:Web3.Contract};
}
export function Factory(url: string, key_store: string, pass: string, 
                        addrs:[{label:string,address:string}], setting: TxSetting):ContractSet {
    var provider: Web3.HttpProvider = ProviderFactory(url, key_store, pass);
    var web3: Web3Class = new Web3Ctor(provider);
    var cs: ContractSet = {
        web3: new Web3Ctor(provider),
        instances:{}
    }
    addrs.forEach((e:{label:string, address:string}) => {
        cs.instances[e.label] = CreateContract(web3, require('./dapp/build/contracts/' + e.label + '.json'), e.address, setting);
    })
    return cs;
}
