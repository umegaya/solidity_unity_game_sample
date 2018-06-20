import { Request, Response } from 'express';
import { Config, Contracts } from '../../common/config';

var sender: string = Config.rpc.addresses[0];

export async function balance(req: Request, res: Response) {
    var address: string = req.body.address;
    try {
        var n_slots: any = Contracts.Inventory.methods.getSlotSize(address).call();
        if (n_slots.toNumber() > 0) {
            var b: any = await Contracts.web3.eth.getBalance(address, null);
            if (b.toNumber() < Contracts.web3.utils.toWei("0.5", "ether")) {
                //if balance is less than 0.5 eth, add balance so that balance is more than 1eth
                await Contracts.web3.eth.sendTransaction({
                    to:address, from:sender, value:Contracts.web3.utils.toWei("0.5", "ether")
                });
            } else {
                //otherwise just return ok
            }
        }
        res.status(200);
        //TODO: returns initial card lists or added token
        res.send({});
    } catch (e) {
        console.log("error balance", e);
        res.status(500);
        res.send(e);
    }
};
