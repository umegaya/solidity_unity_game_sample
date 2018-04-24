var Test = artifacts.require('Test');
var t = require(__dirname + "/../tools/utils/testUtils");


contract('Test', (accounts) => {
    it("test assertion", () => {
        var c;
        return Test.deployed().then((instance) => {
            c = instance;
            return t.tx(c, "testTx", 0, {from: t.addresses(1)});
        }).then((ret) => {
            return t.tx(c, "testTx", 1, {from: t.addresses(1)});
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
        });
    });
});
