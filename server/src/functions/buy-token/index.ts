import { Request, Response } from 'express';

var handler = (req: Request):string => {
    return "buy-token";
}

export function buy_token(req: Request, res: Response) {
    try {
        var out:string = handler(req);
        res.status(200);
        res.send(out);
    } catch (err) {
        res.status(500);
        res.send(err);
    }
};
