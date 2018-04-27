var Inventory = artifacts.require('Inventory');
var World = artifacts.require('World');
var Moritapo = artifacts.require('Moritapo');
var Storage = artifacts.require('Storage');

var soltype = require("soltype-pb");
var protobuf = soltype.importProtoFile(require("protobufjs"));

var helper = require(__dirname + "/../tools/utils/helper");

var NAME = "nyaa";
var NAME2 = "nee";
var HP = 75;
var ATK = 18;
var ATK2 = ATK - 5;
var DEF = 17;
var DEF2 = DEF + 5;
var SKILL_IDS = [11, 12];
var SKILL_IDS2 = [13, 14];
var SKILL_IDS_CHILD = [11, 13];
var catCheck = (cat, opts) => {
    opts = opts || {}
    assert.equal(cat.name, opts.name || NAME, "name should be correct");
    assert.equal(cat.hp.toNumber(), HP, "hp should be correct");
    assert.equal(cat.attack.toNumber(), opts.atk || ATK, "atk should be correct");
    assert.equal(cat.defense.toNumber(), opts.def || DEF, "def should be correct");
    assert.equal(cat.isMale, opts.isMale || false, "is_male should be correct");
    var skills = opts.skills || SKILL_IDS;
    for (var i = 0; i < cat.skills.length; i++) {
        var found = false;  
        for (var j = 0; j < skills.length; j++) {
            if (cat.skills[i].id.toNumber() == skills[j]) {
                found = true;
                break;
            }
        }
        assert(found, "skill id should be found:" + cat.skills[i].id.toNumber());
    }    
}

var pgrs = new helper.Progress();
//pgrs.verbose = true;

contract('Inventory', () => {
    var accounts = Inventory.currentProvider.addresses_;
    writer = accounts[0];
    var base_cat_id;
    it("can create and load cat", () => {
        var c, sc, proto;
        return Storage.deployed().then((instance) => {
            sc = instance;
            return Inventory.deployed();
        }).then((instance) => {
            c = instance;
            return new Promise((resolve, reject) => {
                protobuf.load("proto/Cat.proto", (err, p) => {
                    if (err) { reject(err); }
                    else { 
                        soltype.importTypes(p);
                        resolve(p); 
                    }
                });
            });
        //store / load cat data
        }).then((p) => {
            proto = p;
            //console.log("contract address:", Inventory.address, Moritapo.address, World.address);
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            assert.equal(ret, 0, "account0 should not have any cat");
            return c.addFixedCat(accounts[0], NAME, HP, ATK, DEF, SKILL_IDS, true, {from: writer});
        }).then((ret) => {
            assert.equal(ret.logs.length, 1, "should happen 1 log");
            var log = ret.logs[0];
            assert.equal(log.event, 'AddCat', "event should be AddCat");
            assert.equal(log.args.user, accounts[0], "event should happens on specified account");
            base_cat_id = log.args.id.toString().toNumber();
            var CatProto = proto.lookup("Cat");
            var bs = helper.toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            //console.log("cat", cat);
            catCheck(cat, { isMale: true });
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 1, "slot size should be correct");
            return c.getSlotId.call(accounts[0], 0);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, base_cat_id, "slot id should be correct");
            return c.getSlotId.call(accounts[0], 1);
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
            pgrs.raise(err);
            return c.getSlotBytes.call(accounts[0], 0);
        }).then((ret) => {
            pgrs.step();
            var CatProto = proto.lookup("Cat");
            var cat = CatProto.decode(ret[0].slice(0, ret[1]));
            catCheck(cat, { isMale: true });
            return c.setForSale(accounts[0], 0, 444, {from: writer});
        //set / get price
        }).then((ret) => {
            pgrs.step();
            return c.getPrice(accounts[0], base_cat_id);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 444, "price should be set");
            return c.setForSale(accounts[0], 0, 222, {from: accounts[1]});
        }).then((ret) => {
            assert(false, "should not success with invalid account");
        }, (err) => {
            pgrs.raise(err);
            return c.setForSale(accounts[0], 1, 0, {from: writer});
        //transfer cat
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
            pgrs.raise(err);
            return c.transferCat(accounts[0], accounts[1], base_cat_id, {from: writer});
        }).then((ret) => {
            pgrs.step();
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, 0, "account0 should lose cat");
            return c.getSlotId.call(accounts[1], 0);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret, base_cat_id, "account1 should gain cat which id == base_cat_id");
            return c.getSlotBytes.call(accounts[1], 0);
        }).then((ret) => {
            pgrs.step();
            var CatProto = proto.lookup("Cat");
            var cat = CatProto.decode(ret[0].slice(0, ret[1]));
            catCheck(cat, { isMale: true });
        //breed cat
        }).then((ret) => {
            pgrs.step();
            return c.addFixedCat(accounts[0], NAME, HP, ATK2, DEF2, SKILL_IDS2, false, {from: writer});
        }).then((ret) => {
            pgrs.step();
            var log = ret.logs[0];
            assert.equal(log.event, 'AddCat', "event should be AddCat");
            assert.equal(log.args.id, base_cat_id + 1, "cat id should be correct");
            return c.estimateBreedValue.call(accounts[0], base_cat_id + 1, accounts[1], base_cat_id, 0);
        }).then((ret) => {
            pgrs.step();
            assert.equal(ret.toNumber(), 11500, "value should be correct");
            return c.breed(NAME2, accounts[0], base_cat_id + 1, accounts[1], base_cat_id, 0, {from: writer});
        }).then((ret) => {
            pgrs.step();
            var log = ret.logs[0];
            var CatProto = proto.lookup("Cat");
            var bs = helper.toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            //console.log("cat2", cat);
            assert.equal(log.args.id, base_cat_id + 2, "cat id should be correct");
            catCheck(cat, { atk: ATK, def: DEF2, name: NAME2, skills: SKILL_IDS_CHILD });
            return c.estimateBreedValue.call(accounts[0], base_cat_id + 1, accounts[1], base_cat_id, 16);
        }).then((ret) => {
            assert.equal(ret.toNumber(), 10500, "value should be correct");
            return c.breed(NAME2, accounts[0], base_cat_id + 1, accounts[1], base_cat_id, 16, {from: writer});
        }).then((ret) => {
            var log = ret.logs[0];
            var CatProto = proto.lookup("Cat");
            var bs = helper.toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            assert.equal(log.args.id, base_cat_id + 3, "cat id should be correct");
            catCheck(cat, { atk: ATK2, def: DEF, name: NAME2, skills: SKILL_IDS_CHILD });
        }).then((ret) => {
            return c.addFixedCat(accounts[0], NAME, HP, ATK2, DEF2, SKILL_IDS2, true, {from: writer});
        }).then((ret) => {
            return c.breed(NAME2, accounts[0], base_cat_id + 4, accounts[1], base_cat_id, -1, {from: writer});
        }).then((ret) => { 
            assert(false, "breed should fail by sex is same");
        }, (err) => {
            return c.clearSlots(accounts[0]);
        }).then((ret) => {
            return c.clearSlots(accounts[1]);
        }).then((ret) => {
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => { 
            assert.equal(ret.toNumber(), 0, "slot size should be correct");
            if (pgrs.err_counter != 3) {
                console.log(pgrs.err);  
                assert(false, "should not got halfway throw");
            }
        });
    });
});











