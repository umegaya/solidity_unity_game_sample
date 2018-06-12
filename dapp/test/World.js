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
var cardCheck = (ret, proto, opts) => {
    var log;
    //search AddCard log
    ret.logs.forEach((l) => {
        if (l.event == 'AddCard') { 
            log = l; 
        }
    });
    var CardProto = proto.lookup("Card");
    var bs = helper.toBytes(log.args.created);
    var card = CardProto.decode(bs);
    opts = opts || {}
    if (opts.hp) {
        assert.equal(card.hp.toNumber(), opts.hp, "hp should be correct");
    }
    if (opts.atk) {
        assert.equal(card.attack.toNumber(), opts.atk, "atk should be correct");
    }
    if (opts.defense) {
        assert.equal(card.defense.toNumber(), opts.def, "def should be correct");
    }
    if (opts.skill_len) {
        assert.equal(card.skills.length, opts.skill_len, "skill should be correct");
    }
    return log.args.id.toNumber();
}
var checkSpent = (ret, target) => {
    var log;
    //search AddCard log
    ret.logs.forEach((l) => {
        if (l.event == 'Transfer' && l.args.from == target) { log = l; }
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
    var main_card_id, sub_card_id, merge_card_id;
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
            main_card_id = cardCheck(ret, proto, {
                hp: 75,
                atk: 10,
                def: 10,
                skill_len: 1,
            });
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
        //merge and buy card
            return c.payForInitialCard(0, {from: sub_account, value: 10**18});
        }).then((ret) => {
            pgrs.step();
            sub_card_id = cardCheck(ret, proto);
            sub_balance += 5*(10**3); //already token price doubled above
            return c.estimateMergeFee.call(main_card_id, sub_account, sub_card_id, {from: main_account});
        }).then(async (ret) => {
            pgrs.step();
            est_merge_fee = ret.toNumber();
            assert.equal(est_merge_fee >= 95 && est_merge_fee <= (16 + 95), 
                         true, "merge fee should be correct");
            //allow possible maximum spent
            await tc.approve(World.address, 16 + 95, {from: main_account});
            return c.mergeCard(main_card_id, sub_account, sub_card_id, {from: main_account});
        }).then((ret) => {
            pgrs.step();
            merge_card_id = cardCheck(ret, proto);
            var spent = checkSpent(ret, main_account);
            assert.notEqual(spent, 0, "should spent some merge fee");
            main_balance -= spent;
            return c.getTokenBalance.call({from: main_account});
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), main_balance, "token balance shold be decreased correctly");
            return c.setForSale(0, test_card_price, {from: main_account});
        }).then((ret) => {
            return Inventory.deployed();
        }).then((instance) => {
            pgrs.step();
            return instance.getPrice.call(main_account, main_card_id); //slot 0 should be card_id = 1
        }).then(async (ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), test_card_price, "getPrice should returns correct price");
            await tc.approve(World.address, test_card_price, {from: sub_account});
            return c.buyCard(main_account, main_card_id, { from: sub_account }); //slot 0 should be card_id = 1
        }).then((ret) => {
            pgrs.step();
            sub_balance -= test_card_price;
            return c.getTokenBalance.call({ from: sub_account });
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), sub_balance, "balance should be correctly decreased");
        //reclaim card
            pgrs.step();
            return c.estimateReclaimToken.call(1, { from: sub_account });
        }).then((ret) => {
            pgrs.step();
            reclaim_token = ret.toNumber();
            assert.equal(reclaim_token, 95, "reclaim fee should be correct");
            return c.reclaimToken(1, { from: sub_account });
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
