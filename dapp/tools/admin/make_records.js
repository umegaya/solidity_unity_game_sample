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
const DataContainer = artifacts.require("DataContainer");


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
                columns = r;
                RecordProto = pb.lookup(object_name);
                id_column_name = getIdColumnName(RecordProto);
                console.log('record', object_name, 'column_name', id_column_name);
            } else {
                console.log(csv, 'record', r);
                var obj = RecordProto.create();
                for (var i = 0; i < columns.length; i++) {
                    const c = columns[i];
                    obj[c] = r[i];
                    if (c == id_column_name) {
                        ids.push(toBuffer(r[i]));
                    }
                }
                records.push(RecordProto.encode(obj).finish());
                console.log(records[records.length - 1]);
            }
        });
        //call contract
        const dc = await DataContainer.deployed();
        console.log('addr', dc.address);
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
