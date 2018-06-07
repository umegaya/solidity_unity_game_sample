import { Request, Response } from 'express';
import {MysqlError} from 'mysql';
import { Config, Contracts, DbConn } from '../../common/config';
import * as mysql from 'mysql';
import * as Web3 from 'web3/types.d';

var OnAlreadyRegistered = (address: string, iap_tx: any): Promise<any> => {
    //if iap tx is not stored into database, act like buy-token
    return Contracts.World.methods.buyToken(address, iap_tx.coin_amount).send();
}

var OnNeedRegister = (address: string, iap_tx: any, sel_idx: number, name: string): Promise<any> => {
    //if iap tx is not stored into database, create initial slot and token
    return Contracts.World.methods.createInitialCat(
        address, iap_tx.coin_amount, sel_idx, name, false).send();
}

var Handler = (req: Request, cb: (res: any) => void, ecb: (err: Error) => void) => {
    console.log('body', JSON.stringify(req.body));
    //check req.address already has slot for inventory
    new Promise((resolve: (r: any) => void, reject: (e: Error) => void) => {
        DbConn.query("SELECT * FROM payment WHERE user_id = ?", 
            [req.body.iap_tx.id], (e: MysqlError, r: any) => {
            if (e) { reject(e); }
            else { resolve(r); }
        });
    }).then((r: any) => {
        if (r.length > 0) {
            //if iap tx is stored into database, log it (because it may try replay attack) and return ok 
            cb({});
        } else {
            return Contracts.Inventory.methods.getSlotSize(req.body.address).call({from: req.body.address})
            .then((r: any) => {
                if (r.toNumber() > 0) {
                    //if iap tx is not stored into database, act like buy-token
                    return OnAlreadyRegistered(req.body.address, req.body.iap_tx);
                } else { //if not have (and has transaction)
                    //if iap tx is not stored into database, create initial slot and token
                    return OnNeedRegister(req.body.address, req.body.iap_tx, req.body.sel_idx, req.body.name);
                }
            });
        }
    }, ecb);
}

export function new_account(req: Request, res: Response) {
    Handler(req, (r: any) => {
        console.log(r);
        res.status(200);
        res.send(r);
    }, (err: Error) => {
        console.log(err);
        res.status(500);
        res.send(err);
    });
}
