var World = artifacts.require('World');
var Storage = artifacts.require('Storage');
var Moritapo = artifacts.require('Moritapo');
var Inventory = artifacts.require('Inventory');
var DataContainer = artifacts.require('DataContainer');

const protobuf = require('../tools/utils/pb')([
    "proto/options"
]);

var helper = require(__dirname + "/../tools/utils/helper");
var GameDataCache = require(__dirname + "/../tools/utils/gamedata").Cache;

var TX_ID_1 = "3cb3bd52650132d76c1445926c864e4d808c7db3";
var TX_ID_2 = "dee73cec26ff5678d7e2adc5c41bd22352a27e3f";
var TX_ID_3 = "41bdd8edd2e507b0b30d4a381691284f213a87cc";
var SPEC_ID = 2;
var INSERT_FLAG = 5;
var STARTER_DECK_CARDS = [1, 2, 3, 4], STARTER_DECK_CARD_PRNS = [4, 0, 0, 0];
var cardCheck = (ret, proto, opts) => {
    var log, card;
    //search MintCard log
    ret.logs.forEach((l) => {
        if (l.event == 'MintCard') { 
            var CardProto = proto.lookup("Card");
            var bs = helper.toBytes(l.args.created);
            var card_tmp = CardProto.decode(bs);
            if (card_tmp.specId == opts.spec_id) {
                card = card_tmp;
                log = l; 
            }
        }
    });
    opts = opts || {}
    if (!log) {
        assert(false, "specified spec_id(" + opts.spec_id + ") does not exists");
    }
    assert.equal(card.insertFlags, opts.insert_flags || 0, "insert_flags should be correct:" + JSON.stringify(opts) + "|" + card.insertFlags);
    assert.equal(card.stack, opts.stack, "stack should be correct");
    return [log.args.id.toNumber(), card];
}
var checkSpent = (ret, from, to) => {
    var log;
    //search Transfer log
    ret.logs.forEach((l) => {
        if (l.event == 'Transfer' && l.args.from == from && l.args.to == to) { log = l; }
    });
    return log ? log.args.value : 0;
}

var checkBurn = (ret) => {
    var log;
    //search Transfer log
    ret.logs.forEach((l) => {
        if (l.event == 'Transfer' && l.args.to == "0x0000000000000000000000000000000000000000") { log = l; }
    });
    return log ? log.args.value : 0;
}

var pgrs = new helper.Progress();
//pgrs.verbose = true;

contract('World', (as) => {
    var accounts = World.currentProvider.addresses_;
    var admin_account = accounts[0];
    var main_account = accounts[1]; //because accounts[0] is token creator
    var sub_account = accounts[2];
    var main_card_id, sub_card_id, sub_card_id2;
    var inventory_instance;
    var main_card, sub_card, sub_card2;
    var gdc;
    it("can create and load card", () => {
        var c, sc, tc, proto;
        var est_merge_fee, reclaim_token;
        var main_balance = 0, sub_balance = 0;
        var test_card_price = 2222;
        gdc = new GameDataCache(web3, DataContainer, protobuf);
        return Storage.deployed().then((instance) => {
            sc = instance;
            return World.deployed();
        }).then((instance) => {
            c = instance;
            return Moritapo.deployed();
        }).then((instance) => {
            tc = instance;
            return protobuf.load("proto/Card.proto");
        }).then((p) => {
            proto = p;
            //console.log("contract address:", Inventory.address, Moritapo.address, World.address);
            return c.setPrivilege(sub_account, 2);            
        }).then((ret) => {
            pgrs.step();
            //create initial card (by using main_account)
            return c.createInitialDeck(main_account, TX_ID_1, 10**6, 
                STARTER_DECK_CARDS, STARTER_DECK_CARD_PRNS, {from: main_account});
        }).then((ret) => {
            assert(false, "should not success because main_account is not writer");
        }, (err) => {
            pgrs.raise(err);
            //create initial deck (by using admin_account)
            return c.createInitialDeck(main_account, TX_ID_1, 10**6, 
                STARTER_DECK_CARDS, STARTER_DECK_CARD_PRNS);
        }).then((ret) => {
            pgrs.step();
            var tpl = cardCheck(ret, proto, {
                spec_id: 1,
                insert_flags: 4,
                stack: 0,
            });
            main_card_id = tpl[0];
            main_card = tpl[1];
            //create initial deck (by using admin_account)
            return c.createInitialDeck(main_account, TX_ID_2, 10**6, 
                STARTER_DECK_CARDS, STARTER_DECK_CARD_PRNS);
        }).then((ret) => {
            assert(false, "should not success next create card");
        }, (err) => {
            pgrs.raise(err);
            main_balance += 10**6;
        }).then((ret) => {
            pgrs.step();
            return c.setPrivilege(main_account, 2);
        }).then((ret) => { 
            return c.getTokenBalance.call({from: main_account});
        //buy token
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), main_balance, "gain token balance should correct");
            return c.buyToken(main_account, TX_ID_2, 10**7);
        }).then((ret) => {
            pgrs.step();
            main_balance += 10**7;
        }).then((ret) => {
            return c.getTokenBalance.call({from: main_account});            
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), main_balance, "gain token balance should correct");
            return c.buyToken(main_account, TX_ID_3, 10**6);
        }).then((ret) => {
            pgrs.step();
            main_balance += 5 * (10**5);
            return c.getTokenBalance.call({from: main_account});            
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), main_balance, "gain token balance should correct");
        //buy card
            return c.payForInitialCard(STARTER_DECK_CARDS, STARTER_DECK_CARD_PRNS, {from: sub_account, value: 10**18});
        }).then((ret) => {
            pgrs.step();
            cardCheck(ret, proto, {
                spec_id: 2,
                insert_flags: 0,
                stack: 0,
            });
            var tpl = cardCheck(ret, proto, {
                spec_id: 1,
                insert_flags: 4,
                stack: 0,
            });
            sub_card_id = tpl[0];
            sub_card = tpl[1];
            sub_balance += 5*(10**3); //already token price doubled above
            return c.setForSale(main_card_id, test_card_price, {from: main_account});
        }).then((ret) => {
            return Inventory.deployed();
        }).then((instance) => {
            inventory_instance = instance;
            pgrs.step();
            return inventory_instance.getPrice.call(main_card_id); //slot 0 should be card_id = 1
        }).then(async (ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), test_card_price, "getPrice should returns correct price");
            await inventory_instance.mintFixedCard(main_account, SPEC_ID, INSERT_FLAG, 2, {from: admin_account});
            await tc.approve(World.address, test_card_price, {from: sub_account});
            return c.buyCard(main_account, main_card_id, { from: sub_account }); //slot 0 should be card_id = 1
        }).then((ret) => {
            pgrs.step();
            sub_balance -= test_card_price;
            main_balance += test_card_price;
            return c.getTokenBalance.call({ from: sub_account });
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), sub_balance, "balance should be correctly decreased");
            return c.estimateMergeFee.call(main_card_id, sub_card_id, {from: sub_account});
        }).then(async (ret) => {
            pgrs.step();
            est_merge_fee = ret.toNumber();
            var calc_merge_fee = await helper.estMergeFee(main_card, gdc);
            assert.equal(est_merge_fee, calc_merge_fee, "merge fee should be correct");
            //allow possible maximum spent
            await tc.approve(World.address, est_merge_fee, {from: sub_account});
            return c.mergeCard(main_card_id, sub_card_id, {from: sub_account});
        }).then((ret) => {
            pgrs.step();
            var burned = checkBurn(ret);
            assert.equal(burned, sub_card_id, "sub card should be burned");
            var spent = checkSpent(ret, sub_account, admin_account);
            assert.notEqual(spent, 0, "should spent some merge fee");
            sub_balance -= spent;
            return c.getTokenBalance.call({from: main_account});
        }).then(async (ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), main_balance, "token balance shold be decreased correctly");
            //reclaim card
            pgrs.step();
            var ret2 = await inventory_instance.mintFixedCard(sub_account, SPEC_ID, INSERT_FLAG, 2, {from: admin_account});
            var tpl = cardCheck(ret2, proto,  {
                spec_id: SPEC_ID,
                insert_flags: INSERT_FLAG,
                stack: 2,
            });
            sub_card_id2 = tpl[0];
            sub_card2 = tpl[1];
            return c.estimateReclaimToken.call(sub_card_id2, { from: sub_account });
        }).then(async (ret) => {
            pgrs.step();
            reclaim_token = ret.toNumber();
            var calc_reclaim_token = await helper.estReclaimValue(sub_card2, gdc);
            assert.equal(reclaim_token, calc_reclaim_token, "reclaim fee should be correct");
            return c.reclaimToken(sub_card_id2, { from: sub_account });
        }).then((ret) => {
            pgrs.step();
            sub_balance += reclaim_token;
            return c.getTokenBalance.call({ from: sub_account });        
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), sub_balance, "reclaim should be done correctly");
            if (pgrs.err_counter != 2) {
                console.log(pgrs.err);
                assert(false, "should not got halfway throw");
            }
        });
    });
});
