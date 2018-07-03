/*
    load csv and generate card data. 
    csv can specify how many cards should be issued. 
*/

const fs = require("fs");
const glob = require("glob");
const parse = require('csv-parse')

const cnf = process.env.CONFIG_NAME || process.exit(1);

const csvs = glob.sync(__dirname + "/CardData/*.csv");
csvs.forEach((csv) => {
    const text = fs.readFileSync(csv).toString();
});
