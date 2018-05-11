import { Request, Response } from 'express';

var helloHandler = (req: Request):string => {
    return "buy-token";
}

export function hello(req: Request, res: Response) {
    try {
        var out:string = helloHandler(req);
        res.status(200);
        res.send(out);
    } catch (err) {
        res.status(500);
        res.send(err);
    }
};
