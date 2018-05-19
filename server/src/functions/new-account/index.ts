import { Request, Response } from 'express';
import { Config, Contracts } from '../../common/config';
import * as Web3 from 'web3/types.d';

var Handler = (req: Request, cb: (res: object) => void, ecb: (err: Error) => void) => {
    console.log('body', JSON.stringify(req.body));
    //check req.address already has slot for inventory
    Contracts.Inventory.methods.getSlotSize(req.body.address).call({from: req.body.address})
        .then(cb, ecb);
    //if it has
        //if iap tx is not stored into database, act like buy-token
        //if iap tx is stored into database, log it (because it may try replay attack) and return ok 
    //if not have (and has transaction)
        //if iap tx is not stored into database, create initial slot and token
        //if iap tx is stored into database, log it (because it may try replay attack) and return ok
}

export function new_account(req: Request, res: Response) {
    Handler(req, (r: object) => {
        console.log(r);
        res.status(200);
        res.send(r);
    }, (err: Error) => {
        console.log(err);
        res.status(500);
        res.send(err);
    });
}
