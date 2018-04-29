const fs = require("fs");
const ContractGenerator = require("truffle-contract");
const Contract = ContractGenerator(
	JSON.parse(fs.readFileSync(__dirname + "/../../build/contracts/" + process.argv[2] + ".json"))
);

console.log(process.argv);
const p = require(__dirname + "/../settings/provider");
Contract.setProvider(p);

Contract.at(process.argv[3]).then((c) => {
	return c.setPrivilege(process.argv[4], 1, {from: p.addresses_[0]});
}).then((r) => {
	console.log("done", r);
	if (r.receipt.code != '0x1') {
		process.exit(1);
	}
}, (err) => {
	console.log("err", err);
	process.exit(1);
});
