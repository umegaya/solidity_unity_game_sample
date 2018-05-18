const path = require('path');

module.exports = {
  mode: 'development',
  module: {
    rules: [
        { test: /\.ts$/, use: 'ts-loader' },
        { test: /.*\/build\/contracts\/\.json$/, use: 'raw-loader' },
        { test: /\.pass$/, use: 'raw-loader' },
        { test: /\.ks$/, use: 'raw-loader' },
    ],
  },
  node: {
    fs: "empty",
  },
  resolve: {
    // Add `.ts` and `.tsx` as a resolvable extension.
    extensions: [".ts", ".tsx", ".js"]
  },
  entry: {
    "new-account": './functions/new-account/index.ts',
    "check-transaction": './functions/check-transaction/index.ts',
    "buy-token": './functions/buy-token/index.ts',
  },
  devtool: false,
  output: {
    filename: '[name].js',
    path: path.resolve(path.join(__dirname, '..', '..', 'dist')),
    libraryTarget: 'commonjs',
  }
};
