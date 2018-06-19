import { Request, Response } from 'express';
import { Contracts } from '../../common/config';

export async function buy_token(req: Request, res: Response) {
    var iap_tx: any = req.body.iap_tx;
    var address: string = req.body.address;
    try {
        //TODO: validate iap_tx.id is real tx id
        await Contracts.World.methods.buyToken(address, iap_tx.id, iap_tx.coin_amount).send();
        res.status(200);
        //TODO: returns initial card lists or added token
        res.send({});
    } catch (e) {
        console.log("error buy-token", e);
        res.status(500);
        res.send(e);
    }
};
