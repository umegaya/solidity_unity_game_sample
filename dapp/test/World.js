var World = artifacts.require('World');
var Storage = artifacts.require('Storage');
var Moritapo = artifacts.require('Moritapo');
var Inventory = artifacts.require('Inventory');

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
var catCheck = (ret, proto, opts) => {
    var log;
    //search AddCat log
    ret.logs.forEach((l) => {
        if (l.event == 'AddCat') { log = l; }
    });
    var CatProto = proto.lookup("Cat");
    var bs = toBytes(log.args.created);
    var cat = CatProto.decode(bs);
    opts = opts || {}
    if (opts.name) {
        assert.equal(cat.name, opts.name, "name should be correct");
    }
    if (opts.hp) {
        assert.equal(cat.hp.toNumber(), opts.hp, "hp should be correct");
    }
    if (opts.atk) {
        assert.equal(cat.attack.toNumber(), opts.atk, "atk should be correct");
    }
    if (opts.defense) {
        assert.equal(cat.defense.toNumber(), opts.def, "def should be correct");
    }
    if (opts.skill_len) {
        assert.equal(cat.skills.length, opts.skill_len, "skill should be correct");
    }
}
var checkSpent = (ret, target) => {
    var log;
    //search AddCat log
    ret.logs.forEach((l) => {
        if (l.event == 'Transfer' && l.args.from == target) { log = l; }
    });
    return log ? log.args.value : 0;
}


contract('World', (accounts) => {
    console.log(accounts);
    var admin_account = accounts[0];
    var main_account = accounts[1]; //because accounts[0] is token creator
    var sub_account = accounts[2];
    it("can create and load cat", () => {
        var c, sc, tc, proto;
        var no_throw = false;
        var est_breed_fee, reclaim_token;
        var main_balance = 0, sub_balance = 0;
        var test_cat_price = 2222;
        return Storage.deployed().then((instance) => {
            sc = instance;
            return World.deployed();
        }).then((instance) => {
            c = instance;
            return Moritapo.deployed();
        }).then((instance) => {
            tc = instance;
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
            return c.setPrivilege(main_account, 1);
        }).then((ret) => {
            return c.setPrivilege(sub_account, 1);
        }).then((ret) => {
            //create initial cat (by using main_account)
            return c.createInitialCat(main_account, 10**6, 0, "cat0", true, {from: main_account});
        }).then((ret) => {
            assert(false, "should not success because main_account is not writer");
        }, (err) => {
            //create initial cat (by using admin_account)
            return c.createInitialCat(main_account, 10**6, 0, "cat0", true);
        }).then((ret) => {
            console.log(1);
            catCheck(ret, proto, {
                name: "cat0",
                hp: 75,
                atk: 10,
                def: 10,
                skill_len: 1,
            });
            return c.createInitialCat(main_account, 10**6, 0, "cat0", true);
        }).then((ret) => {
            assert(false, "should not success next create cat");
        }, (err) => {
            console.log(2, err);
            main_balance += 10**6;
            return c.getTokenBalance.call({from: main_account});
        //buy token
        }).then((ret) => {
            console.log(3);
            assert.equal(ret.toNumber(), main_balance, "gain token balance should correct");
            return c.buyToken(main_account, 10**7);
        }).then((ret) => {
            console.log(4);
            main_balance += 10**7;
            return c.getTokenBalance.call({from: main_account});            
        }).then((ret) => {
            console.log(5);
            assert.equal(ret.toNumber(), main_balance, "gain token balance should correct");
            return c.buyToken(main_account, 10**6);
        }).then((ret) => {
            console.log(6);
            main_balance += 5 * (10**5);
            return c.getTokenBalance.call({from: main_account});            
        }).then((ret) => {
            console.log(7, ret);
            assert.equal(ret.toNumber(), main_balance, "gain token balance should correct");
        //breed and buy cat
            return c.payForInitialCat(0, "cat0", false, {from: sub_account, value: 10**18});
        }).then((ret) => {
            console.log(8);
            proess.exit(1);
            sub_balance += 5*(10**3); //already token price doubled above
            return c.estimateBreedFee.call(1, sub_account, 2, {from: main_account});
        }).then(async (ret) => {
            est_breed_fee = ret.toNumber();
            assert.equal(est_breed_fee >= 95 && est_breed_fee <= (16 + 95), 
                         true, "breed fee should be correct");
            //allow possible maximum spent
            await tc.approve(World.address, 16 + 95, {from: main_account});
            return c.breedCat("breed", 1, sub_account, 2, {from: main_account});
        }).then((ret) => {
            console.log(9);
            catCheck(ret, proto, {
                name: "breed",
            });
            var spent = checkSpent(ret, main_account);
            assert.notEqual(spent, 0, "should spent some breed fee");
            main_balance -= spent;
            return c.getTokenBalance.call({from: main_account});
        }).then((ret) => {
            console.log(10);
            assert.equal(ret.toNumber(), main_balance, "token balance shold be decreased correctly");
            return c.setForSale(0, test_cat_price, {from: main_account});
        }).then((ret) => {
            return Inventory.deployed();
        }).then((instance) => {
            console.log(11);
            return instance.getPrice.call(main_account, 1); //slot 0 should be cat_id = 1
        }).then(async (ret) => {
            console.log(12);
            assert.equal(ret.toNumber(), test_cat_price, "getPrice should returns correct price");
            await tc.approve(World.address, test_cat_price, {from: sub_account});
            return c.buyCat(main_account, 1, { from: sub_account }); //slot 0 should be cat_id = 1
        }).then((ret) => {
            console.log(13);
            sub_balance -= test_cat_price;
            return c.getTokenBalance.call({ from: sub_account });
        }).then((ret) => {
            console.log(14);
            assert.equal(ret.toNumber(), sub_balance, "balance should be correctly decreased");
        //reclaim cat
        console.log(15);
        return c.estimateReclaimToken.call(1, { from: sub_account });
        }).then((ret) => {
            console.log(16);
            reclaim_token = ret.toNumber();
            assert.equal(reclaim_token, 95, "reclaim fee should be correct");
            return c.reclaimToken(1, { from: sub_account });
        }).then((ret) => {
            console.log(17);
            sub_balance += reclaim_token;
            no_throw = true;
            return c.getTokenBalance.call({ from: sub_account });        
        }).then((ret) => {
            console.log(18);
            assert.equal(ret.toNumber(), sub_balance, "reclaim should be done correctly");
        });
    });    
});
