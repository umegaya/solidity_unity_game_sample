const fs = require('fs');
const pbjs = require('protobufjs');

var paths = [];
pbjs.Root.prototype.paths = paths;
pbjs.Root.prototype.resolvePath = function pbjsPathResolver(origin, target) {
  // Borrow from
  // https://github.com/dcodeIO/protobuf.js/blob/master/cli/pbjs.js
  var normOrigin = pbjs.util.path.normalize(origin),
      normTarget = pbjs.util.path.normalize(target);

  var resolved = pbjs.util.path.resolve(normOrigin, normTarget, true);
  var idx = resolved.lastIndexOf("google/protobuf/");
  if (idx > -1) {
    var altname = resolved.substring(idx);
    if (altname in pbjs.common) {
      resolved = altname;
    }
  }

  if (fs.existsSync(resolved)) {
    return resolved;
  }

  for (var i = 0; i < paths.length; ++i) {
    var iresolved = pbjs.util.path.resolve(paths[i] + "/", target);
    if (fs.existsSync(iresolved)) {
      return iresolved;
    }
  }
};
