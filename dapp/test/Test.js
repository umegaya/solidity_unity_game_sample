const ethers = require("ethers");
const Test = artifacts.require('Test');

contract('Test', async () => {
    it("test array bytes argument func", async () => {
        const tmpc = await Test.deployed();
        const dc = new ethers.Contract(tmpc.address, tmpc.abi, 
            new ethers.providers.Web3Provider(web3.currentProvider));
        var bs = [Buffer.from('abc', 'utf8'), Buffer.from('def', 'utf8')];
        const ret = await dc.foo(bs);
        assert (ret.toNumber() == 2);
        const ret2 = await dc.foo([]);
        assert (ret2.toNumber() == 0);
    });
});
