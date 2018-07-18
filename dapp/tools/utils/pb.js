module.exports = function (paths) {
    const soltype = require("soltype-pb");
    const protobuf = require("protobufjs");
    require('./pbjs-path-resolver');
    //import solidity native type
    soltype.importProtoFile(protobuf);
    //add protobuf include path
    (paths || []).forEach((p) => {
        protobuf.Root.prototype.paths.push(p);
    });
    const origload = protobuf.load;
    protobuf.load = function () {
        var pb = origload(arguments);
        if (typeof(pb) == 'object' && typeof(pb.then) == 'function') {
            pb.then((loaded) => {
                soltype.importTypes(loaded);
                return loaded;
            });
        } else {
            soltype.importTypes(pb);
        }
        return pb;
    }
    return protobuf;
}
