var Inventory = artifacts.require('Inventory');
var Storage = artifacts.require('Storage');

var soltype = require("soltype-pb");
var protobuf = soltype.importProtoFile(require("protobufjs"));

var t = require(__dirname + "/../tools/utils/testUtils");

var toBytes = (hexdump) => {
    var buff = new Uint8Array(hexdump.length / 2 - 1);
    var idx = 0;
    hexdump.substring(2).replace(/\w{2}/g, (m) => {
        buff[idx++] = parseInt(m, 16);
    });
    return buff;
}

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
var prog_num = 0;
var progress = (msg) => {
    prog_num++;
    console.log(prog_num, msg);
}

contract('Inventory', () => {
    var accounts = t.addresses;
    writer = accounts[0];
    it("can create and load cat", () => {
        var c, sc, proto, no_throw = false;
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
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            assert.equal(ret, 0, "account0 should not have any cat");
            return t.tx(c, "addFixedCat", accounts[0], NAME, HP, ATK, DEF, SKILL_IDS, true, {from: writer});
        }).then((ret) => {
            assert.equal(ret.logs.length, 1, "should happen 1 log");
            var log = ret.logs[0];
            assert.equal(log.event, 'AddCat', "event should be AddCat");
            assert.equal(log.args.user, accounts[0], "event should happens on specified account");
            assert.equal(log.args.id, 1, "cat id should be correct");
            var CatProto = proto.lookup("Cat");
            var bs = toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            //console.log("cat", cat);
            catCheck(cat, { isMale: true });
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            progress();
            assert.equal(ret, 1, "slot size should be correct");
            return c.getSlotId.call(accounts[0], 0);
        }).then((ret) => {
            progress();
            assert.equal(ret, 1, "slot id should be correct");
            return c.getSlotId.call(accounts[0], 1);
        }).then((ret) => {
            progress();
            assert(false, "should not success with invalid index");
        }, (err) => {
            progress();
            return c.getSlotBytes.call(accounts[0], 0);
        }).then((ret) => {
            progress();
            var CatProto = proto.lookup("Cat");
            var cat = CatProto.decode(ret[0].slice(0, ret[1]));
            catCheck(cat, { isMale: true });
            return t.tx(c, "setForSale", accounts[0], 0, 444, {from: writer});
        //set / get price
        }).then((ret) => {
            progress();
            return c.getPrice(accounts[0], 1);
        }).then((ret) => {
            progress();
            assert.equal(ret, 444, "price should be set");
            return t.tx(c, "setForSale", accounts[0], 0, 222, {from: accounts[1]});
        }).then((ret) => {
            progress();
            assert(false, "should not success with invalid account");
        }, (err) => {
            progress();
            return t.tx(c, "setForSale", accounts[0], 1, 0, {from: writer});
        //transfer cat
        }).then((ret) => {
            progress("assert");
            assert(false, "should not success with invalid index");
        }, (err) => {
            progress();
            return t.tx(c, "transferCat", accounts[0], accounts[1], 1, {from: writer});
        }).then((ret) => {
            progress();
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            progress();
            assert.equal(ret, 0, "account0 should lose cat");
            return c.getSlotId.call(accounts[1], 0);
        }).then((ret) => {
            progress();
            assert.equal(ret, 1, "account1 should gain cat which id == 1");
            return c.getSlotBytes.call(accounts[1], 0);
        }).then((ret) => {
            progress();
            var CatProto = proto.lookup("Cat");
            var cat = CatProto.decode(ret[0].slice(0, ret[1]));
            catCheck(cat, { isMale: true });
        //breed cat
        }).then((ret) => {
            progress();
            return t.tx(c, "addFixedCat", accounts[0], NAME, HP, ATK2, DEF2, SKILL_IDS2, false, {from: writer});
        }).then((ret) => {
            progress();
            var log = ret.logs[0];
            assert.equal(log.event, 'AddCat', "event should be AddCat");
            assert.equal(log.args.id, 2, "cat id should be correct");
            return c.estimateBreedValue.call(accounts[0], 2, accounts[1], 1, 0);
        }).then((ret) => {
            progress();
            assert.equal(ret.toNumber(), 11500, "value should be correct");
            return t.tx(c, "breed", NAME2, accounts[0], 2, accounts[1], 1, 0, {from: writer});
        }).then((ret) => {
            progress();
            var log = ret.logs[0];
            var CatProto = proto.lookup("Cat");
            var bs = toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            //console.log("cat2", cat);
            assert.equal(log.args.id, 3, "cat id should be correct");
            catCheck(cat, { atk: ATK, def: DEF2, name: NAME2, skills: SKILL_IDS_CHILD });
            return c.estimateBreedValue.call(accounts[0], 2, accounts[1], 1, 16);
        }).then((ret) => {
            assert.equal(ret.toNumber(), 10500, "value should be correct");
            return t.tx(c, "breed", NAME2, accounts[0], 2, accounts[1], 1, 16, {from: writer});
        }).then((ret) => {
            var log = ret.logs[0];
            var CatProto = proto.lookup("Cat");
            var bs = toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            assert.equal(log.args.id, 4, "cat id should be correct");
            catCheck(cat, { atk: ATK2, def: DEF, name: NAME2, skills: SKILL_IDS_CHILD });
        }).then((ret) => {
            return t.tx(c, "addFixedCat" , accounts[0], NAME, HP, ATK2, DEF2, SKILL_IDS2, true, {from: writer});
        }).then((ret) => {
            no_throw = true;
            return t.tx(c, "breed", NAME2, accounts[0], 5, accounts[1], 1, -1, {from: writer});
        }).then((ret) => { 
            assert(false, "breed should fail by sex is same");
        }, (err) => {
            if (!no_throw) {
                console.log(err);
                assert(false, "should not got halfway throw");
            }
        });
    });
});











