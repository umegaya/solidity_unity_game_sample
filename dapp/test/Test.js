var Test = artifacts.require('Test');


contract('Test', () => {
    var accounts = Test.currentProvider.addresses_;
    var no_throw = false;
    it("test assertion", () => {
        var c;
        return Test.deployed().then((instance) => {
            c = instance;
            return c.testTx(0, {from: accounts[1]});
        }).then((ret) => {
            no_throw = true;
            return c.testTx(1, {from: accounts[2]});
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
