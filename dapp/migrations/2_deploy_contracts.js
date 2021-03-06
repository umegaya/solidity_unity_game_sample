var Storage = artifacts.require("Storage");
var CalcUtil = artifacts.require("CalcUtil");
var Moritapo = artifacts.require("Moritapo");
var Inventory = artifacts.require("Inventory");
var World = artifacts.require("World");
var Issuance = artifacts.require("Issuance");
var User = artifacts.require("User");
var History = artifacts.require("History");
var Cards = artifacts.require("Cards");
var DataContainer = artifacts.require("DataContainer");
var Test = artifacts.require("Test");

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
    return deployer.deploy(Test);
  }).then(function () {
    return deployer.deploy(CalcUtil);
  }).then(function () {
    return deployer.deploy(Cards);
  }).then(function () {
    return deployer.deploy(DataContainer, Storage.address);
  }).then(function () {
    return deployer.deploy(Issuance, Storage.address);
  }).then(function () {
    return deployer.deploy(User, Storage.address);
  }).then(function () {
    Inventory.link(CalcUtil);
    return deployer.deploy(Inventory, Storage.address, Cards.address, Issuance.address);
  }).then(function () {
    World.link(CalcUtil);
    return deployer.deploy(World, Moritapo.address, Inventory.address, User.address);
  }).then(function () {
    return Storage.at(Storage.address).then(function (instance) {
      return instance.setPrivilege(Inventory.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return Storage.at(Storage.address).then(function (instance) {
      return instance.setPrivilege(DataContainer.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return Cards.at(Cards.address).then(function (instance) {
      return instance.setPrivilege(Inventory.address, PRIV_WRITABLE);
    })
  }).then(function () {
    return Inventory.at(Inventory.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return Moritapo.at(Moritapo.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  }).then(function () {
    return DataContainer.at(DataContainer.address).then(function (instance) {
      return instance.setPrivilege(World.address, PRIV_WRITABLE);
    });
  });
};
