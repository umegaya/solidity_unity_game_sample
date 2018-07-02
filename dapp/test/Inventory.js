var Inventory = artifacts.require('Inventory');
var World = artifacts.require('World');
var Moritapo = artifacts.require('Moritapo');
var Storage = artifacts.require('Storage');

var soltype = require("soltype-pb");
var protobuf = soltype.importProtoFile(require("protobufjs"));

var helper = require(__dirname + "/../tools/utils/helper");

var TX_ID_1 = "7ce7d7a5c855b3ede91b37bda8d460e1c77ec4a1";
var TX_ID_2 = "0abbbed04b081b5c22517e2216d3cf8cfce00a97";
var HP = 75;
var ATK = 18;
var ATK2 = ATK - 5;
var DEF = 17;
var DEF2 = DEF + 5;
var SKILL_IDS = [11, 12];
var SKILL_IDS2 = [13, 14];
var SKILL_IDS_CHILD = [11, 13];
var cardCheck = (card, opts) => {
    opts = opts || {}
    assert.equal(card.hp.toNumber(), HP, "hp should be correct");
    assert.equal(card.attack.toNumber(), opts.atk || ATK, "atk should be correct");
    assert.equal(card.defense.toNumber(), opts.def || DEF, "def should be correct");
    var skills = opts.skills || SKILL_IDS;
    for (var i = 0; i < card.skills.length; i++) {
        var found = false;  
        for (var j = 0; j < skills.length; j++) {
            if (card.skills[i].id.toNumber() == skills[j]) {
                found = true;
                break;
            }
        }
        assert(found, "skill id should be found:" + card.skills[i].id.toNumber());
    }    
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
    var accounts = Inventory.currentProvider.addresses_;
    writer = accounts[0];
    var base_card_id, base_card_id2, remain_card_id;
    it("can create and load card", () => {
        var c, sc, proto;
        return Storage.deployed().then((instance) => {
            sc = instance;
            return Inventory.deployed();
        }).then((instance) => {
            c = instance;
            return new Promise((resolve, reject) => {
                protobuf.load("proto/Card.proto", (err, p) => {
                    if (err) { reject(err); }
                    else { 
                        soltype.importTypes(p);
                        resolve(p); 
                    }
                });
            });
        //store / load card data
        }).then((p) => {
            pgrs.step();
            proto = p;
            //console.log("contract address:", Inventory.address, Moritapo.address, World.address);
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 0, "account0 should not have any card");
            return c.mintFixedCard(accounts[0], HP, ATK, DEF, SKILL_IDS, {from: writer});
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
            return c.mintFixedCard(accounts[0], HP, ATK, DEF, SKILL_IDS, {from: writer});
        }).then((ret) => {
            pgrs.step();
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
            return c.mintFixedCard(accounts[1], HP, ATK2, DEF2, SKILL_IDS2, {from: writer});
        }).then((ret) => {
            pgrs.step();
            var log = ret.logs[0];
            assert.equal(log.event, 'MintCard', "event should be MintCard");
            remain_card_id = base_card_id + 2;
            assert.equal(log.args.id, remain_card_id, "card id should be correct");
            return c.estimateResultValue.call(remain_card_id, base_card_id, 0);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), 11500, "value should be correct");
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
            cardCheck(card, { atk: ATK, def: DEF2, skills: SKILL_IDS_CHILD });
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











