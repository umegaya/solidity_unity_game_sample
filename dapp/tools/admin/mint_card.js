const fs = require("fs");
const parse = require('csv-parse/lib/sync');
const protobuf = require("../utils/pb")([
    __dirname + "/../../proto",
    __dirname + "/../../proto/options"
]);

const text = fs.readFileSync(csv).toString();
const raw_records = parse(text);

