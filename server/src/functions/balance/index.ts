import { Request, Response } from 'express';
import { Config, Contracts, DbConn } from '../../common/config';

var CheckAndFillBalance = (address: string): Promise<any> => {
    return Contracts.web3.eth.getBalance(address, null).then((result: number) => {
        if (result < Contracts.web3.toWei("0.5", "ether")) {
            //if balance is less than 0.5 eth, add balance so that balance is more than 1eth
            
        } else {
            //otherwise just return ok
        }
    });
}

var Handler = (req: Request, cb: (res: any) => void, ecb: (err: Error) => void) => {
    //check address has slot
    return Contracts.Inventory.methods.getSlotSize(req.body.address).call({from: req.body.address})
    .then((r: any) => {
        if (r.toNumber() > 0) {    //if does have
            return CheckAndFillBalance(req.body.address);
        } else { //if does not have, just return ok
            cb({});
        }
    }, ecb);
}

export function balance(req: Request, res: Response) {
    Handler(req, (r: any) => {
        console.log(r);
        res.status(200);
        res.send(r);
    }, (err: Error) => {
        console.log(err);
        res.status(500);
        res.send(err);
    });
};
