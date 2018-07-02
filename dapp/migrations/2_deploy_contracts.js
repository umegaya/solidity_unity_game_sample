var Storage = artifacts.require("Storage");
var CalcUtil = artifacts.require("CalcUtil");
var Moritapo = artifacts.require("Moritapo");
var Inventory = artifacts.require("Inventory");
var World = artifacts.require("World");
var History = artifacts.require("History");
var Cards = artifacts.require("Cards");

function deploy_pb(deployer) {
	var PbRuntime = artifacts.require("_pb");
	var CatCodec = artifacts.require("pb_ch_Card");
	var TownCodec = artifacts.require("pb_ch_Town");
	var UserCodec = artifacts.require("pb_ch_User");
	var MatchCodec = artifacts.require("pb_ch_Match");
	return deployer.deploy(PbRuntime).then(function () {
		CatCodec.link(PbRuntime);
		return deployer.deploy(CatCodec);
	}).then(function () {
		TownCodec.link(PbRuntime);
		return deployer.deploy(TownCodec);
	}).then(function () {
		UserCodec.link(PbRuntime);
		return deployer.deploy(UserCodec);
	}).then(function () {
		MatchCodec.link(PbRuntime);
		return deployer.deploy(MatchCodec);
	});
}

var PRIV_WRITABLE = 2;

module.exports = function(deployer) {
  deployer.deploy(Storage)
  /*.then(function() {
    return deploy_pb();
  })//*/
  .then(function () {
    return deployer.deploy(Moritapo);
  }).then(function () {
    return deployer.deploy(CalcUtil);
  }).then(function () {
    return deployer.deploy(Cards);
  }).then(function () {
    Inventory.link(CalcUtil);
    return deployer.deploy(Inventory, Storage.address, Cards.address);
  }).then(function () {
    World.link(CalcUtil);
    return deployer.deploy(World, Moritapo.address, Inventory.address);
  }).then(function () {
    return Storage.at(Storage.address).then(function (instance) {
      return instance.setPrivilege(Inventory.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return Inventory.at(Inventory.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return Moritapo.at(Moritapo.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return Cards.at(Cards.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return World.at(World.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  });
};
