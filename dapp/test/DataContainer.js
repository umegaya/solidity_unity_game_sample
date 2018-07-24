const protobuf = require('../tools/utils/pb')();
const EthersContract = require("../tools/utils/ethers").Contract;
const DataContainer = artifacts.require('DataContainer');

const numToBuffer = (n) => {
    var b = new Buffer([n]);
    return b;
}

const ID_FIXTURE = [
    numToBuffer(1),
    numToBuffer(2),
    numToBuffer(3),
    numToBuffer(4),
    numToBuffer(5),
    numToBuffer(6),
    numToBuffer(7),
    numToBuffer(8),
    numToBuffer(9),
];
const DATA_FIXTURE = [
    new Buffer('abc1', 'utf8'),
    new Buffer('def22', 'utf8'),
    new Buffer('ghi333', 'utf8'),
    new Buffer('jkl4444', 'utf8'),
    new Buffer('mno55555', 'utf8'),
    new Buffer('pqr666666', 'utf8'),
    new Buffer('stu7777777', 'utf8'),
    new Buffer('vwx88888888', 'utf8'),
    new Buffer('yz_999999999', 'utf8'),
];
const DATA_FIXTURE2 = [
    new Buffer('cba1', 'utf8'),
    new Buffer('fed22', 'utf8'),
    new Buffer('ihg333', 'utf8'),
];

const parseLogs = (receipt, events) => {
    var txEvents = {}
    for (const log of receipt.logs) {
        // for each event in the ABI
        for (const abiEvent of Object.values(events))
        {
            // if the hash of the ABI event equals the tx receipt log
            if (abiEvent.topics[0] == log.topics[0])
            {
                // Parse the event from the log topics and data
                if (!txEvents[abiEvent.name]) {
                    txEvents[abiEvent.name] = [abiEvent.parse(log.topics, log.data)];
                } else {
                    txEvents[abiEvent.name].push(abiEvent.parse(log.topics, log.data));
                }

                // stop looping through the ABI events
                break
            }
        }
    }
    return txEvents;
}

contract('DataContainer', async () => {
    it("can put/get records", async () => {
        const overrideOptions = {
            gasLimit: 32000000,
        };
        const dc = await EthersContract(DataContainer, web3);
        const tx1 = await dc.putRecords("Test", ID_FIXTURE, DATA_FIXTURE, overrideOptions);
        await dc.provider.waitForTransaction(tx1.hash);
        const receipt1 = await dc.provider.getTransactionReceipt(tx1.hash);
        //console.log('receipt1', receipt1, parseLogs(receipt1, dc.interface.events)["Payload"]);
        assert(receipt1.status != 0, "tx1 fail to execute contract");
        const tx2 = await dc.putRecords("Test", [
            numToBuffer(9),
            numToBuffer(10),
            numToBuffer(11),                    
        ], DATA_FIXTURE2, overrideOptions);
        await dc.provider.waitForTransaction(tx2.hash);
        const receipt2 = await dc.provider.getTransactionReceipt(tx2.hash);
        //console.log('receipt2', receipt2);
        assert(receipt2.status != 0, "tx2 fail to execute contract");

        const expected0 = [];
        DATA_FIXTURE.forEach((d) => {
            expected0.push(d);
        });
        expected0[8] = DATA_FIXTURE2[0]; 
        expected0[9] = DATA_FIXTURE2[1]; 
        expected0[10] = DATA_FIXTURE2[2]; 

        const gen0_diff_result = await dc.recordIdDiff("Test", 0);
        assert.equal(gen0_diff_result[0].toNumber(), 2, "gen0 curgen wrong");
        assert.equal(gen0_diff_result[1][0].length, 11, "gen0 idlist length wrong");
        const gen0_get_records = await dc.getRecords("Test", gen0_diff_result[1][0]);
        for (var i = 0; i < gen0_diff_result.length; i++) {
            var rec = gen0_get_records[i]; //this is hexdump string
            assert.equal(rec.substring(2), expected0[i].toString('hex'), "gen0 returned record wrong"); 
        }

        const expected1 = DATA_FIXTURE2;
        const gen1_diff_result = await dc.recordIdDiff("Test", 1);
        //console.log(gen1_diff_result);
        assert.equal(gen1_diff_result[0].toNumber(), 2, "gen1 curgen wrong");
        assert.equal(gen1_diff_result[1][0].length, 3, "gen1 idlist length wrong");
        const gen1_get_records = await dc.getRecords("Test", gen1_diff_result[1][0]);
        for (var i = 0; i < gen1_diff_result.length; i++) {
            var rec = gen1_get_records[i]; //this is hexdump string
            assert.equal(rec.substring(2), expected1[i].toString('hex'), "gen1 returned record wrong"); 
        }
    });
});
