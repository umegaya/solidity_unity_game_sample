const EthersContract = require('./ethers').Contract;
const helper = require('./helper');

class Cache {
    constructor(web3, data_container, proto) {
        this.data_container = data_container;
        this.web3 = web3;
        this.proto = proto;
        this.protoDefs = {};
        this.dataCache = {};
    }

    async getData(type, id) {
        if (!this.ethers_dc) {
            this.ethers_dc = await EthersContract(this.data_container, this.web3);
        }
        if (!this.protoDefs[type]) {
            var p = await this.proto.load("proto/templates/" + type + ".proto");
            this.protoDefs[type] = p.lookup(type);
        }
        if (!this.dataCache[type]) {
            this.dataCache[type] = {}
        }
        var collection = this.dataCache[type];
        if (!collection[id]) {
            var specs = await this.ethers_dc.getRecords(type, [this.toIdBytes(id)]);
            console.log(specs);
            collection[id] = this.protoDefs[type].decode(helper.toBytes(specs[0]));
        }
        return collection[id];
    }

    toIdBytes(id) {
        if (typeof(id) == "number") {
            return helper.numToBytes(id);
        } else if (typeof(id) == "string") {
            return Buffer.from(id, "utf8");
        }
    } 
}

module.exports = {
    Cache: Cache,
};