var fs = require('fs');
var path = require('path');

var functions = {};
var dirs = fs.readdirSync(__dirname);
for (var i = 0; i < dirs.length; i++) {
    console.log("dir:" + i + " => " + dirs[i]);
    var p = path.join(__dirname, dirs[i]);
    if (fs.statSync(p).isDirectory()) {
        //TODO: interpolate parcel-built javascript file path and require
        functions = Object.assign(functions, require(path.join(p, "build")));
    }
}

module.exports = functions;
