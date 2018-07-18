var fs = require("fs");

var cnf = process.env.CONFIG_NAME || process.exit(1);

var Moritapo = artifacts.require("Moritapo");
var Inventory = artifacts.require("Inventory");
var World = artifacts.require("World");
var DataContainer = artifacts.require("DataContainer");
//var History = artifacts.require("History");

var addresses = [
	{label: "World", address:World.address},
	{label: "Inventory", address:Inventory.address},
	{label: "Moritapo", address:Moritapo.address},
	{label: "DataContainer", address:DataContainer.address},
];

module.exports = function(finish) {
	fs.writeFileSync(__dirname + "/../../build/addresses/" +  cnf + ".json", JSON.stringify(addresses));
	finish();
}
