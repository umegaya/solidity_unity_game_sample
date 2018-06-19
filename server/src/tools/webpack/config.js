const path = require('path');
const glob = require('glob');
const nodeExternals = require('webpack-node-externals');

const getEntries = () => {
  var paths = glob.sync("./functions/*/index.ts");
  var ret = {}
  paths.map((path) => {
    var m = path.match(/^\.\/functions\/([^\/]+)/);
    ret[m[1]] = path;
  });
  return ret;
};

module.exports = {
  mode: process.env.CONFIG_NAME == "prod" ? 'production' : 'development',
  module: {
    rules: [
        { test: /\.ts$/, use: 'ts-loader' },
        { test: /\.pass$/, use: 'raw-loader' },
        { test: /\.ks$/, use: 'json-loader' },
    ],
  },
  resolve: {
    // Add `.ts` and `.tsx` as a resolvable extension.
    extensions: [".ts", ".tsx", ".js"],
  },
  externals: [nodeExternals()],
  entry: getEntries(),
  devtool: false,
  target: 'node',
  output: {
    filename: '[name]/index.js',
    path: path.resolve(path.join(__dirname, '..', '..', 'dist')),
    libraryTarget: 'commonjs',
  },
};
