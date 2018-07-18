const protobuf = require('../tools/utils/pb')();
const DataContainer = artifacts.require('DataContainer');

contract('DataContainer', async () => {
    it("can put/get records", async () => {
        const dc = await DataContainer.deployed();
        var bs = [new Buffer('abc', 'utf8')];
        console.log(bs[0]);
        const ret = await dc.putRecords('Hoge', [], []);
        console.log(ret.logs[0], ret.logs[0].args.code.toString());
    });
});
