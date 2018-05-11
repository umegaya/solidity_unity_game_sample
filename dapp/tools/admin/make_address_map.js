var fs = require("fs");

var cnf = process.env.CONFIG_NAME || process.exit(1);

var Moritapo = artifacts.require("Moritapo");
var Inventory = artifacts.require("Inventory");
var World = artifacts.require("World");

var addresses = {
	World: World.address,
	Inventory: Inventory.address,
	Moritapo: Moritapo.address
};

fs.writeFileSync("../../build/contracts/__" +  cnf + "__addresses__.json", JSON.stringify(addresses));
