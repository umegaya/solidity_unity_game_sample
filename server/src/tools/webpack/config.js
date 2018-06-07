const path = require('path');
const nodeExternals = require('webpack-node-externals');

module.exports = {
  mode: 'development',
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
  entry: {  
    "new-account": './functions/new-account/index.ts',
    "check-transaction": './functions/check-transaction/index.ts',
    "buy-token": './functions/buy-token/index.ts',
  },
  devtool: false,
  target: 'node',
  output: {
    filename: '[name].js',
    path: path.resolve(path.join(__dirname, '..', '..', 'dist')),
    libraryTarget: 'commonjs',
  },
};
