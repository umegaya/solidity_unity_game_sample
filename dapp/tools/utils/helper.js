class Progress {
    constructor() {
        this.counter = 0;
        this.err_counter = 0;
        this.err = null;
        this.verbose = false;
    }
    step(msg) {
        this.counter++;
        if (msg) {
            if (this.verbose) console.log(this.counter,msg);
        } else {
            if (this.verbose) console.log(this.counter);
        }
    }
    raise(err) {
        this.err = err;
        this.err_counter++;
        if (this.verbose) console.log('throw', this.counter, this.err_counter, err.stack);
    }
}


var toBytes = (hexdump) => {
    var buff = new Uint8Array(hexdump.length / 2 - 1);
    var idx = 0;
    hexdump.substring(2).replace(/\w{2}/g, (m) => {
        buff[idx++] = parseInt(m, 16);
    });
    return buff;
}

module.exports = {
    toBytes: toBytes,
    Progress: Progress,
}
