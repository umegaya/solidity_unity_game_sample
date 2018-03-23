#!/bin/bash
node /usr/local/lib/node_modules/truffle/build/chain.bundled.js develop '{"accounts":[{"balance":1e21,"secretKey":"0xc87509a1c067bbde78beb793e6fa76530b6382a4c0241e5e4a9ec0a0f44dc0d3"},{"balance":1e21,"secretKey":"'$1'"}],"host":"localhost","port":9545,"network_id":4447,"mnemonic":"candy maple cake sugar pudding cream honey rich smooth crumble sweet treat","gasLimit":6721975}'
