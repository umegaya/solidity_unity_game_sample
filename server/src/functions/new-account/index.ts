import { Request, Response } from 'express';
import { Config, Contracts } from '../../common/config';

var Handler = (req: Request, cb: (resp: object) => void) => {
    //check req.address already has slot for inventory
    Contracts.Inventory.methods.getSlotSize(req.body.address, cb);
    //if it has
        //if iap tx is not stored into database, act like buy-token
        //if iap tx is stored into database, log it (because it may try replay attack) and return ok 
    //if not have (and has transaction)
        //if iap tx is not stored into database, create initial slot and token
        //if iap tx is stored into database, log it (because it may try replay attack) and return ok
}

export function new_account(req: Request, res: Response) {
    try {
        Handler(req, (out: object) => {
            res.status(200);
            res.send(out);
        });
    } catch (err) {
        res.status(500);
        res.send(err);
    }
}
