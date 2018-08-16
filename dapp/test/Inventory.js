var Inventory = artifacts.require('Inventory');
var World = artifacts.require('World');
var Moritapo = artifacts.require('Moritapo');
var Storage = artifacts.require('Storage');
var DataContainer = artifacts.require('DataContainer');

const protobuf = require('../tools/utils/pb')([
    "proto/options"
]);

var helper = require(__dirname + "/../tools/utils/helper");
var GameDataCache = require(__dirname + "/../tools/utils/gamedata").Cache;

var TX_ID_1 = "7ce7d7a5c855b3ede91b37bda8d460e1c77ec4a1";
var TX_ID_2 = "0abbbed04b081b5c22517e2216d3cf8cfce00a97";
var SPEC_ID = 1;
var INSERT_FLAG = 5;
var INSERT_FLAG2 = 4;
var RARITY = 3;
var cardCheck = (card, opts) => {
    opts = opts || {}
    assert.equal(card.specId, opts.spec_id || SPEC_ID, "spec_id should be correct");
    if (opts.insert_flags !== false) {
        assert.equal(card.insertFlags, opts.insert_flags || INSERT_FLAG, "insert_flags should be correct");
    }
    assert.equal(card.stack, opts.stack || 0, "level should be correct");
}
var consumeCheck = (ret) => {
    //search ConsumeTx log
    for (var i = 0; i < ret.logs.length; i++) {
        var l = ret.logs[i];
        if (l.event == 'ConsumeTx') { 
            return true;
        }
    }
    return false;
}

var pgrs = new helper.Progress();
//pgrs.verbose = true;

contract('Inventory', () => {
    var gdc;
    var accounts = Inventory.currentProvider.addresses_;
    writer = accounts[0];
    var base_card_id, card_tmp, remain_card_id;

    it("can create and load card", () => {
        var c, sc, dc, proto;
        gdc = new GameDataCache(web3, DataContainer, protobuf);
        return Storage.deployed().then((instance) => {
            sc = instance;
            return Inventory.deployed();
        }).then((instance) => {
            c = instance;
            return DataContainer.deployed();
        }).then((instance) => {
            dc = instance;
            return protobuf.load("proto/Card.proto");
        //store / load card data
        }).then((p) => {
            pgrs.step();
            proto = p;
            //console.log("contract address:", Inventory.address, Moritapo.address, World.address);
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 0, "account0 should not have any card");
            return c.mintFixedCard(accounts[0], SPEC_ID, INSERT_FLAG, 0, {from: writer});
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.logs.length, 1, "should happen 1 log");
            var log = ret.logs[0];
            assert.equal(log.event, 'MintCard', "event should be MintCard");
            assert.equal(log.args.user, accounts[0], "event should happens on specified account");
            base_card_id = Number(log.args.id.toString());
            var CardProto = proto.lookup("Card");
            var bs = helper.toBytes(log.args.created);
            var card = CardProto.decode(bs);
            //console.log("card", card);
            cardCheck(card);
            return c.mintFixedCard(accounts[0], SPEC_ID, INSERT_FLAG, 0, {from: writer});   
        }).then((ret) => {
            pgrs.step();
            var log = ret.logs[0];
            base_card_id2 = Number(log.args.id.toString());
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 2, "slot size should be correct");
            return c.getSlotId.call(accounts[0], 0);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, base_card_id, "slot id should be correct");
            return c.getSlotId.call(accounts[0], 2);
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
            pgrs.raise(err);
            return c.getSlotBytes.call(accounts[0], 0);
        }).then((ret) => {
            pgrs.step();
            var CardProto = proto.lookup("Card");
            var bs = helper.toBytes(ret);
            var card = CardProto.decode(bs);
            cardCheck(card);
            return c.setForSale(accounts[0], base_card_id, 444, {from: writer});
        //set / get price
        }).then((ret) => {
            pgrs.step();
            return c.getPrice(base_card_id);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 444, "price should be set");
            return c.setForSale(accounts[0], base_card_id, 222, {from: accounts[1]});
        }).then((ret) => {
            assert(false, "should not success with invalid from account");
        }, (err) => {
            pgrs.raise(err);
            return c.setForSale(accounts[0], base_card_id + 10, 0, {from: writer});
        //transfer card
        }).then((ret) => {
            assert(false, "should not success with invalid id");
        }, (err) => {
            pgrs.raise(err);
            return c.transferCard(accounts[0], accounts[1], base_card_id, {from: writer});
        }).then((ret) => {
            pgrs.step();
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 1, "account0 should lose card");
            return c.getSlotId.call(accounts[1], 0);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, base_card_id, "account1 should gain card which id == base_card_id");
            return c.getSlotBytes.call(accounts[1], 0);
        }).then((ret) => {
            pgrs.step();
            var CardProto = proto.lookup("Card");
            var bs = helper.toBytes(ret);
            var card = CardProto.decode(bs);
            cardCheck(card);
        //merge card
        }).then((ret) => {
            pgrs.step();
            return c.mintFixedCard(accounts[1], SPEC_ID, INSERT_FLAG2, 0, {from: writer});
        }).then((ret) => {
            pgrs.step();
            var log = ret.logs[0];
            var CardProto = proto.lookup("Card");
            var bs = helper.toBytes(log.args.created);
            card_tmp = CardProto.decode(bs);
            assert.equal(log.event, 'MintCard', "event should be MintCard");
            remain_card_id = base_card_id + 2;
            assert.equal(log.args.id, remain_card_id, "card id should be correct");
            return c.estimateResultValue.call(remain_card_id, base_card_id);
        }).then(async (ret) => {
            pgrs.step();
            var est_price = await helper.estPrice(card_tmp, gdc, true);
            assert.equal(ret.toNumber(), est_price, "value should be correct");
            return c.merge(accounts[1], remain_card_id, base_card_id, 0, {from: writer});
        }).then((ret) => {
            pgrs.step();
            var log = ret.logs[0];
            var CardProto = proto.lookup("Card");
            var bs = helper.toBytes(log.args.created);
            var card = CardProto.decode(bs);
            //console.log("card2", card);
            assert.equal(log.args.remain_card_id, remain_card_id, "remain card id should be correct");
            assert.equal(log.args.merged_card_id, base_card_id, "merged card id should be correct");
            cardCheck(card, { insert_flags: false, stack: 1 });
            return c.getSlotSize.call(accounts[1]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 1, "account0 should burn card");
            return c.clearSlots(accounts[1], {from: writer});
        }).then((ret) => {
            pgrs.step();
            return c.getSlotSize.call(accounts[1]);
        }).then((ret) => { 
            assert.equal(ret.toNumber(), 0, "account1 slot size should be zero because of clear slot");
            return c.recordPayment(accounts[0], TX_ID_1);
        }).then((ret) => {
            pgrs.step();
            assert(consumeCheck(ret));
            return c.recordPayment(accounts[0], TX_ID_2);
        }).then((ret) => {
            pgrs.step();
            assert(consumeCheck(ret));
            return c.recordPayment(accounts[0], TX_ID_1);
        }).then((ret) => {
            assert(!consumeCheck(ret), "should not be able to use same tx_id twice");
            return c.recordPayment(accounts[1], TX_ID_1);            
        }).then((ret) => {
            assert(ret);
            if (pgrs.err_counter != 3) {
                console.log(pgrs.err);  
                assert(false, "should not got halfway throw");
            }
        });
    });
});











