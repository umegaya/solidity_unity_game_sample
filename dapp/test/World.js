var World = artifacts.require('World');
var Storage = artifacts.require('Storage');
var Moritapo = artifacts.require('Moritapo');
var Inventory = artifacts.require('Inventory');

var soltype = require("soltype-pb");
var protobuf = soltype.importProtoFile(require("protobufjs"));

var helper = require(__dirname + "/../tools/utils/helper");

var TX_ID_1 = "3cb3bd52650132d76c1445926c864e4d808c7db3";
var TX_ID_2 = "dee73cec26ff5678d7e2adc5c41bd22352a27e3f";
var TX_ID_3 = "41bdd8edd2e507b0b30d4a381691284f213a87cc";
var SPEC_ID = 5678;
var VISUAL_FLAG = 5;
var RARITY = 4;
var cardCheck = (ret, proto, opts) => {
    var log;
    //search MintCard log
    ret.logs.forEach((l) => {
        if (l.event == 'MintCard') { 
            log = l; 
        }
    });
    var CardProto = proto.lookup("Card");
    var bs = helper.toBytes(log.args.created);
    var card = CardProto.decode(bs);
    opts = opts || {}
    assert.equal(card.specId, opts.spec_id || SPEC_ID, "spec_id should be correct");
    assert.equal(card.visualFlags, typeof(opts.visual_flags) == 'number' ? opts.visual_flags : VISUAL_FLAG, "visual_flags should be correct:" + JSON.stringify(opts));
    assert.equal(card.level, opts.level || 2, "level should be correct");
    assert.equal(Number(card.bs[0]), opts.rarity || RARITY, "rarity should be correct");
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
    it("can create and load card", () => {
        var c, sc, tc, proto;
        var est_merge_fee, reclaim_token;
        var main_balance = 0, sub_balance = 0;
        var test_card_price = 2222;
        return Storage.deployed().then((instance) => {
            sc = instance;
            return World.deployed();
        }).then((instance) => {
            c = instance;
            return Moritapo.deployed();
        }).then((instance) => {
            tc = instance;
            return new Promise((resolve, reject) => {
                protobuf.load("proto/Card.proto", (err, p) => {
                    if (err) { reject(err); }
                    else { 
                        soltype.importTypes(p);
                        resolve(p); 
                    }
                });
            });
        }).then((p) => {
            proto = p;
            //console.log("contract address:", Inventory.address, Moritapo.address, World.address);
            return c.setPrivilege(sub_account, 2);            
        }).then((ret) => {
            pgrs.step();
            //create initial card (by using main_account)
            return c.createInitialDeck(main_account, TX_ID_1, 10**6, 0, {from: main_account});
        }).then((ret) => {
            assert(false, "should not success because main_account is not writer");
        }, (err) => {
            pgrs.raise(err);
            //create initial deck (by using admin_account)
            return c.createInitialDeck(main_account, TX_ID_1, 10**6, 0);
        }).then((ret) => {
            pgrs.step();
            var tpl = cardCheck(ret, proto, {
                spec_id: 1,
                visual_flags: 0,
                level: 1,
                rarity: 4,
            });
            main_card_id = tpl[0];
            main_card = tpl[1];
            //create initial deck (by using admin_account)
            return c.createInitialDeck(main_account, TX_ID_2, 10**6, 0);
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
            return c.payForInitialCard(0, {from: sub_account, value: 10**18});
        }).then((ret) => {
            pgrs.step();
            var tpl = cardCheck(ret, proto, {
                spec_id: 1,
                visual_flags: 0,
                level: 1,
                rarity: 4,
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
            await inventory_instance.mintFixedCard(main_account, SPEC_ID, VISUAL_FLAG, 2, RARITY, {from: admin_account});
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
            assert.equal(est_merge_fee, helper.estMergeFee(main_card), "merge fee should be correct");
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
            var ret2 = await inventory_instance.mintFixedCard(sub_account, SPEC_ID, VISUAL_FLAG, 2, RARITY, {from: admin_account});
            var tpl = cardCheck(ret2, proto);
            sub_card_id2 = tpl[0];
            sub_card2 = tpl[1];
            return c.estimateReclaimToken.call(sub_card_id2, { from: sub_account });
        }).then((ret) => {
            pgrs.step();
            reclaim_token = ret.toNumber();
            assert.equal(reclaim_token, helper.estReclaimValue(sub_card2), "reclaim fee should be correct");
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
