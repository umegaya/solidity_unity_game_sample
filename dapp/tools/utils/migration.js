var config = {};
module.exports = {
    extend: function(deployer, migration_contract) {
        var origDeploy = deployer.deploy.bind(deployer);
        deployer.deploy = function (contract) {
            var promise = origDeploy(...arguments);
            return new Promise(function (resolve, reject) {
                promise.then(function (result) {
                    config.instance = result;
                    console.log("set deployed address:" + contract.contract_name + " => " + contract.address);
                    return migration_contract.setDeployedAddress(contract.contractName, contract.address);
                }).then(function (result) {
                    resolve(config.instance);
                }, reject);
            });
        };
        return deployer;
    }
}
