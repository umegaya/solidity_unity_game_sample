var Inventory = artifacts.require('Inventory');
var Storage = artifacts.require('Storage');

var soltype = require("soltype-pb");
var protobuf = soltype.importProtoFile(require("protobufjs"));

var toBytes = (hexdump) => {
    var buff = new Uint8Array(hexdump.length / 2 - 1);
    var idx = 0;
    hexdump.substring(2).replace(/\w{2}/g, (m) => {
        buff[idx++] = parseInt(m, 16);
    });
    return buff;
}

var NAME = "nyaa";
var HP = 75;
var ATK = 18;
var DEF = 17;
var SKILL_IDS = [11, 12];
var catCheck = (cat) => {
    assert.equal(cat.name, NAME, "name should be correct");
    assert.equal(cat.hp.toNumber(), HP, "hp should be correct");
    assert.equal(cat.attack.toNumber(), ATK, "atk should be correct");
    assert.equal(cat.defense.toNumber(), DEF, "def should be correct");
    for (var i = 0; i < cat.skills.length; i++) {
        assert.equal(cat.skills[i].id.toNumber(), SKILL_IDS[i], "skills should be correct");
    }    
}

contract('Inventory', (accounts) => {
    it("can create and load cat", () => {
        var c, sc, proto;
        var inventory_watcher;
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
        }).then((p) => {
            proto = p;
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            assert.equal(ret, 0, "account0 should not have any cat");
            return c.addFixedCat(accounts[0], NAME, HP, ATK, DEF, SKILL_IDS);
        }).then((ret) => {
            assert.equal(ret.logs.length, 1, "should happen 1 log");
            var log = ret.logs[0];
            assert.equal(log.event, 'AddCat', "event should be AddCat");
            assert.equal(log.args.user, accounts[0], "event should happens on specified account");
            assert.equal(log.args.id, 1, "cat id should be correct");

            var CatProto = proto.lookup("Cat");
            var bs = toBytes(log.args.created);
            var cat = CatProto.decode(bs);
            catCheck(cat);
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            assert.equal(ret, 1, "slot size should be correct");
            return c.getSlotId.call(accounts[0], 0);
        }).then((ret) => {
            assert.equal(ret, 1, "slot id should be correct");
            return c.getSlotId.call(accounts[0], 1);
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
            return c.getSlotBytes.call(account[0], 0);
        }).then((ret) => {
            var CatProto = proto.lookup("Cat");
            var cat = CatProto.decode(ret[0].slice(0, ret[1]));
            catCheck(cat);
            return c.setForSale(accounts[0], 0, 444);
        }).then((ret) => {
            return c.getPrice(accounts[0], 0);
        }).then((ret) => {
            assert.equal(ret, 444, "price should be set");
            return c.setForSale(accounts[1], 0, 222);
        }).then((ret) => {
            assert(false, "should not success with invalid account");
        }, (err) => {
            return c.setForSale(account[0], 1, 0);
        }).then((ret) => {
            assert(false, "should not success with invalid index");
        }, (err) => {
            return c.transferCat(accounts[0], accounts[1], 1);
        }).then((ret) => {
            return c.getSlotSize.call(accounts[0]);
        }).then((ret) => {
            assert.equal(ret, 0, "account0 should lose cat");
            return c.getSlotId.call(accounts[1], 0);
        }).then((ret) => {
            assert.equal(ret, 1, "account1 should gain cat which id == 1");
            return c.getSlotBytes.call(accounts[1], 0);
        }).then((ret) => {
            var CatProto = proto.lookup("Cat");
            var cat = CatProto.decode(ret[0].slice(0, ret[1]));
            catCheck(cat);
        });
    });
});











