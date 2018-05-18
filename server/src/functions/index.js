var fs = require('fs');
var path = require('path');
var functions = {};
var dirs = fs.readdirSync(__dirname);
for (var i = 0; i < dirs.length; i++) {
	var d = dirs[i];
    var p = path.join(__dirname, d);
    if (fs.statSync(p).isDirectory()) {
	    console.log("dir:" + i + " => " + d + "(" + path.join("..", "dist", d) + ")");
        //TODO: interpolate parcel-built javascript file path and require
        var req = require(path.join("..", "dist", d));
        functions = Object.assign(functions, req);
    }
}

module.exports = functions;
