import { Request, Response } from 'express';
import { Config, Contracts } from '../../common/config';

var sender: string = Config.wallet_address;
var World = Contracts.instances["World"];
var Inventory = Contracts.instances["Inventory"];
var Eth = Contracts.web3.eth;

export async function new_account(req: Request, res: Response) {
    var iap_tx: any = req.body.iap_tx;
    var selected_idx: number = req.body.selected_idx;
    var address: string = req.body.address;
    var n_slot: number = await Inventory.methods.getSlotSize(address).call();
    try {
        //TODO: validate iap_tx.id is real tx id
        if (n_slot > 0) {
            //if already has registered, act like buy-token
            await World.methods.buyToken(address, iap_tx.id, iap_tx.coin_amount).send();
        } else {
            //create deck
            await World.methods.createInitialDeck(
                address, iap_tx.id, iap_tx.coin_amount, selected_idx).send();
            //send initial fuel
            await Eth.sendTransaction({
                to:address, from:sender, value:Contracts.web3.utils.toWei("1", "ether")
            });
        }
        res.status(200);
        //TODO: returns initial card lists or added token
        res.send({});
    } catch (e) {
        console.log("error new-account", e);
        res.status(500);
        res.send(e);
    }
}
