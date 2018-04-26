var Test = artifacts.require('Test');
var t = require(__dirname + "/../tools/utils/testUtils");


contract('Test', () => {
    var accounts = t.addresses;
    t.set_web3(web3);
    var no_throw = false;
    it("test assertion", () => {
        var c;
        return Test.deployed().then((instance) => {
            c = instance;
            return t.tx(Test, c, "testTx", 0, {from: accounts[1]});
        }).then((ret) => {
            no_throw = true;
            return t.tx(Test, c, "testTx", 1, {from: accounts[2]});
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
            if (!no_throw) {
                console.log(err);
                assert(false, "got halfway throw");
            }
        });
    });
});
