import { Request, Response } from 'express';
import { Config, Contracts } from '../../common/config';

var sender: string = Config.wallet_address;
var Inventory = Contracts.instances["Inventory"];
var Eth = Contracts.web3.eth;

var BALANCE_ADDED = Contracts.web3.utils.toWei("0.5", "ether");
var BALANCE_ADDED_THRESHOLD = Contracts.web3.utils.toWei("0.5", "ether");

export async function balance(req: Request, res: Response) {
    var address: string = req.body.address;
    var current_balance: number = 0;
    try {
        var n_slots: number = await Inventory.methods.getSlotSize(address).call();
        if (n_slots > 0) {
            current_balance = await Eth.getBalance(address, null);
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
        console.log("current_balance:", current_balance);
        res.status(200);
        res.send({
            balance: current_balance.toString()
        });
    } catch (e) {
        console.log("error balance", e);
        res.status(500);
        res.send(e);
    }
};
