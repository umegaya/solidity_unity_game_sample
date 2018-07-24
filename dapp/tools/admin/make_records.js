/*
    load csv and generate card data. 
    csv can specify how many cards should be issued. 
*/

const fs = require("fs");
const glob = require("glob");
const parse = require('csv-parse/lib/sync');
const protobuf = require("../utils/pb")([
    __dirname + "/../../proto",
    __dirname + "/../../proto/options"
]);
const ethers_util = require("../utils/ethers");
const helper = require("../utils/helper");
const DataContainer = artifacts.require('DataContainer');
console.log("DataContainer address:" + DataContainer.address);

const ethers_connection = {
    contract: false
};

const cnf = process.env.CONFIG_NAME || process.exit(1);

const csvs = glob.sync(__dirname + "/Data/*.csv");
console.log('csvs', csvs);

const ID_OPTION_NAME = "(suntomi.pb.field_options).id"
const getIdColumnName = (proto) => {
    for (var k in proto.fields) {
        const f = proto.fields[k];
        if (f.options && f.options[ID_OPTION_NAME]) {
            //camelize
            return k.replace(/_(%w)/g, (m, a1) => {
                return a1.toUpperCase();
            });
        }
    }
    return false;
}
const toBuffer = (src) => {
    var t = typeof(src);
    if (t == 'string') {
        return Buffer.from(src, 'utf8');
    } else if (t == 'number') {
        return helper.numToBytes(src);
    } else if (t instanceof Buffer) {
        return src;
    } else {
        var e = 'unsupported type:' + t;
        console.log(e);
        throw e;
    }
}

const toValue = (ft, val) => {
    if (ft.startsWith('uint') || ft.startsWith('int') || ft == 'double' || ft == 'float') {
        return Number(val);
    } else if (ft == 'bool') {
        return (val == 'true');
    } else if (ft == 'bytes') {
        return new Buffer(val, 'hex');
    } else if (ft == 'string') {
        return val;
    } else {
        throw new Error(`invalid type ${ft}`);
    }  
}

const setValueByField = (obj, field, val) => {
    if (val.length <= 0) {
        return;
    }
    const ft = field.type;
    if (field.rule == 'repeated') {
        obj[field.name].push(toValue(ft, val));
    } else {
        obj[field.name] = toValue(ft, val);
    }
}

const initConnection = async (web3) => {
    ethers_connection.contract = await ethers_util.Contract(DataContainer, web3);
}

const logging = false;
const log = function () {
    if (logging) {
        console.log(arguments);
    }
}


const registerCSV = async (csv) => {
    try {
        const object_name = csv.match(/\/([^/]+)\.csv/)[1];
        log(csv, object_name);
        const text = fs.readFileSync(csv).toString();
        const raw_records = parse(text);
        //get protobuf definition. note that this load is override
        //to import solidity native type automatically
        const pb = await protobuf.load(`templates/${object_name}.proto`);
        //create contract payload
        var columns = null, field_columns, RecordProto = null, id_column_name = null;
        var records = [], ids = [];
        raw_records.forEach((r) => {
            if (!columns) {
                field_columns = r.map((c) => {
                    return c.substring(0, 1).toLowerCase() + c.substring(1);
                })
                columns = r.map((c) => {
                    return c.substring(0, 1).toLowerCase() + c.substring(1).replace(/([A-Z]+)/, (m, a1) => {
                        return ("_" + a1.toLowerCase());
                    });
                });
                RecordProto = pb.lookup(object_name);
                id_column_name = getIdColumnName(RecordProto);
            } else {
                var obj = RecordProto.create();
                log(csv, 'record', r, obj);
                for (var i = 0; i < columns.length; i++) {
                    const c = columns[i];
                    const fc = field_columns[i];
                    const f = RecordProto.fields[fc];
                    if (!f) {
                        console.log("field not exists:", RecordProto.fields, fc);
                        throw new Error(csv + ": field not exists:" + fc);
                    }
                    setValueByField(obj, f, r[i]);
                    if (c == id_column_name) {
                        var bs = toBuffer(toValue(f.type, r[i]));
                        if (bs.length <= 0) {
                            throw new Error(`invalid id for ${object_name} id = ${r[i]}`);
                        }
                        ids.push(bs);
                    }
                }
                var b = RecordProto.encode(obj).finish();
                if (b.length <= 0) {
                    throw new Error(`invalid object ${object_name} id = ${obj[id_column_name]}@${id_column_name}`);
                } else {
                    log("obj", obj, "encoded length", b.length);
                }
                records.push(b);
                log(records[records.length - 1]);
            }
        });
        //call contract
        const conn = ethers_connection.contract;
        const tx = await conn.putRecords(object_name, ids, records, ethers_util.overrideOptions);
        log(tx);
        await conn.provider.waitForTransaction(tx.hash);
        const receipt = await conn.provider.getTransactionReceipt(tx.hash);    
        if (receipt.status == 0) {
            console.log(receipt);
            throw new Error(`transaction fails ${receipt.status}`);
        }
        /*const ret = await conn.recordIdDiff(object_name, 0);
        if (ret[1][0].length != ids.length) {
            throw new Error(`record length invalid ${ret[1][0].length} vs ${ids.length}`)
        }   
        const ret = await conn.getRecords(object_name, ids);
        for (var i = 0; i < ret.length; i++) {
            log('getRecords', i, RecordProto.decode(helper.toBytes(ret[i])));
        }*/
        console.log(object_name, ids.length, "records are registered");
    } catch (e) {
        console.log("process csv columns error", e);
    }
}

module.exports = async function(finish) {
    await initConnection(web3);
    //await Promise.all(promises);
    for (var i = 0; i < csvs.length; i++) {
        await registerCSV(csvs[i]);
    }
	finish();
}
