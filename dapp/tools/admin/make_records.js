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
const ethers = require("ethers");
const DataContainer = artifacts.require('DataContainer');

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
        return Buffer.from(src.toString(16), 'hex');
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

var promises = csvs.map(async (csv) => {
    try {
        console.log(csv);
        const object_name = csv.match(/\/([^/]+)\.csv/)[1];
        console.log(csv, object_name);
        const text = fs.readFileSync(csv).toString();
        const raw_records = parse(text);
        //get protobuf definition. note that this load is override
        //to import solidity native type automatically
        const pb = await protobuf.load(`templates/${object_name}.proto`);
        //create contract payload
        var columns = null, RecordProto = null, id_column_name = null;
        var records = [], ids = [];
        raw_records.forEach((r) => {
            if (!columns) {
                columns = r.map((c) => {
                    return c.substring(0, 1).toLowerCase() + c.substring(1).replace(/([A-Z]+)/, (m, a1) => {
                        return ("_" + a1.toLowerCase());
                    });
                });
                RecordProto = pb.lookup(object_name);
                id_column_name = getIdColumnName(RecordProto);
                console.log('record', object_name, 'column_name', id_column_name);
            } else {
                var obj = RecordProto.create();
                console.log(csv, 'record', r, obj);
                for (var i = 0; i < columns.length; i++) {
                    const c = columns[i];
                    const f = RecordProto.fields[c];
                    const ct = typeof(obj[c]);
                    setValueByField(obj, f, r[i]);
                    if (c == id_column_name) {
                        ids.push(toBuffer(r[i]));
                    }
                }
                console.log('obj => ', obj);
                var b = RecordProto.encode(obj).finish();
                if (b.length <= 0) {
                    throw new Error(`invalid object ${object_name} id = ${obj[id_column_name]}@${id_column_name}`);
                }
                records.push(b);
                console.log(records[records.length - 1]);
            }
        });
        //call contract
        const tmpc = await DataContainer.deployed();
        if (!web3.eth.provider) {
            web3.eth.provider = web3.eth.currentProvider;
        }
        const dc = new ethers.Contract(tmpc.address, tmpc.abi, web3.eth);
        const ret = await dc.putRecords(object_name, ids, records);
        console.log(ret.logs);
        console.log(object_name, ids.length, "records are registered");
    } catch (e) {
        console.log("process csv columns error", e);
    }
});

module.exports = async function(finish) {
    await Promise.all(promises);
	finish();
}
