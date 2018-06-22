import { Request, Response } from 'express';
import { Config, Contracts } from '../../common/config';

var sender: string = Config.rpc.addresses[0];
var Inventory = Contracts.instances["Inventory"];
var Eth = Contracts.web3.eth;

var BALANCE_ADDED = Contracts.web3.utils.toWei("0.5", "ether");
var BALANCE_ADDED_THRESHOLD = Contracts.web3.utils.toWei("0.5", "ether");

export async function balance(req: Request, res: Response) {
    var address: string = req.body.address;
    var current_balance: number = 0;
    try {
        var n_slots: any = await Inventory.methods.getSlotSize(address).call();
        if (n_slots.toNumber() > 0) {
            var b: any = await Eth.getBalance(address, null);
            current_balance = b.toNumber();
            if (current_balance < BALANCE_ADDED_THRESHOLD) {
                //if balance is less than 0.5 eth, add balance so that balance is more than 1eth
                await Eth.sendTransaction({
                    to:address, from:sender, value:BALANCE_ADDED
                });
                current_balance += BALANCE_ADDED;
            } else {
                //otherwise just return ok
            }
        }
        res.status(200);
        res.send({
            balance: current_balance
        });
    } catch (e) {
        console.log("error balance", e);
        res.status(500);
        res.send(e);
    }
};
