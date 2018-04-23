var Storage = artifacts.require("Storage");
var NekoUtil = artifacts.require("NekoUtil");
var Moritapo = artifacts.require("Moritapo");
var Inventory = artifacts.require("Inventory");
var World = artifacts.require("World");
var Test = artifacts.require("Test");

function deploy_pb(deployer) {
	var PbRuntime = artifacts.require("_pb");
	var CatCodec = artifacts.require("pb_neko_Cat");
	var TownCodec = artifacts.require("pb_neko_Town");
	return deployer.deploy(PbRuntime).then(function () {
		CatCodec.link(PbRuntime);
		return deployer.deploy(CatCodec);
	}).then(function () {
		TownCodec.link(PbRuntime);
		return deployer.deploy(TownCodec);
	});
}

module.exports = function(deployer) {
  deployer.deploy(Storage)
  /*.then(function() {
    return deploy_pb();
  })//*/
  .then(function () {
    return deployer.deploy(Moritapo);
  }).then(function () {
    return deployer.deploy(NekoUtil);
  }).then(function () {
    return deployer.deploy(Test);
  }).then(function () {
    Inventory.link(NekoUtil);
    return deployer.deploy(Inventory, Storage.address);
  }).then(function () {
    World.link(NekoUtil);
    return deployer.deploy(World, Moritapo.address, Inventory.address);
  }).then(function () {
    return Storage.at(Storage.address).then(function (instance) {
      return instance.setPrivilege(Inventory.address, 1);
    });
  }).then(function () {
    return Inventory.at(Inventory.address).then(function (instance) {
      return instance.setPrivilege(World.address, 1);
    });
  }).then(function () {
    return Moritapo.at(Moritapo.address).then(function (instance) {
      return instance.setPrivilege(World.address, 1);
    });
  }).then(function () {
    return World.at(World.address).then(function (instance) {
      return instance.setPrivilege(World.address, 1);
    });
  });
};
