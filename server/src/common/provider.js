const fs = require('fs');
const ProviderClass = require('truffle-privatekey-provider');
const keythereum = require("keythereum");

module.exports = function (url, key_store, pass) {
    var pkey = keythereum.recover(pass, key_store);
    return new ProviderClass(pkey, url);
}
