const cp = require('child_process');

class Progress {
    constructor() {
        this.counter = 0;
        this.err_counter = 0;
        this.err = null;
        this.verbose = false;
    }
    step(msg) {
        this.counter++;
        if (msg) {
            if (this.verbose) console.log('prg', this.counter,msg);
        } else {
            if (this.verbose) console.log('prg', this.counter);
        }
    }
    raise(err) {
        this.err = err;
        this.err_counter++;
        if (this.verbose) console.log('throw', this.counter, this.err_counter, err.stack);
    }
}

var chop = function (str) {
    if (typeof(str) != 'string') {
        str = str.toString();
    }
    var i = str.length - 1;
    for (; i >= 0; i--) {
        if (str.charAt(i) != '\r' && str.charAt(i) != '\n') {
            break;
        }
    }
    if (i < (str.length - 1)) {
        return str.substring(0, i+1);
    } else {
        return str;
    }
}

var toBytes = (hexdump) => {
    var buff = new Uint8Array(hexdump.length / 2 - 1);
    var idx = 0;
    hexdump.substring(2).replace(/\w{2}/g, (m) => {
        buff[idx++] = parseInt(m, 16);
    });
    return buff;
}

var numToBytes = (num) => {
    var buff = [];
    while (num != 0) {
        buff.push(num & 0xFF);
        num >>= 8;
    }
    return new Buffer(buff);
}

var getDockerHost = () => {
    if (process.env.DOCKER_HOST) {
        //docker machine
        return process.env.DOCKER_HOST.replace(/tcp:\/\/([0-9\.]+):.*/, function (m, a1) { console.log(m, a1); return a1; });
    } else {
        //docker for mac/win
        return "localhost";
    }
}

var getMinikubeHost = () => {
    return chop(cp.execSync("minikube ip"));
}

var numberOfSetBits = (i) => {
    i = i - ((i >> 1) & 0x55555555);
    i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
    return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
}

const createEthersContract = async (truffle_contract, web3) => {
    const tmpc = await truffle_contract.deployed();
    const wallet = await getWallet();
    wallet.provider = new ethers.providers.Web3Provider(web3.currentProvider);
    return new ethers.Contract(tmpc.address, tmpc.abi, wallet);
}

var estPrice = async (card, gdc, after_merge) => {
    if (after_merge) {
        card.stack++;
    }
    var spec = await gdc.getData("CardSpec", card.specId);
    var est = 100 * (1 << Number(spec.rarity)) * (1 << card.stack) * (1 + numberOfSetBits(card.insertFlags));
    if (after_merge) {
        card.stack--;
    }
    return est;
}

var estMergeFee = async (card, gdc) => {
    return await estPrice(card, gdc, true) / 100;
}

var estReclaimValue = async (card, gdc) => {
    return await estPrice(card, gdc, false) / 100;
}

module.exports = {
    chop: chop,
    toBytes: toBytes,
    numToBytes: numToBytes,
    Progress: Progress,
    getDockerHost: getDockerHost,
    getMinikubeHost: getMinikubeHost,
    numberOfSetBits: numberOfSetBits,
    createEthersContract: createEthersContract,
    estPrice: estPrice,
    estMergeFee: estMergeFee,
    estReclaimValue: estReclaimValue,
}
