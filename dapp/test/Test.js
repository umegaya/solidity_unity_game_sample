var Test = artifacts.require('Test');

contract('Test', (accounts) => {
    it("test assertion", () => {
        var c;
        return Test.deployed().then((instance) => {
            c = instance;
            return c.testTx(0);
        }).then((ret) => {
            return c.testTx(1);
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
        });
    });
});
